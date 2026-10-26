namespace ResolveHub.Services.Implementations;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ResolveHub.Core.DTOs.Auth;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;
using ResolveHub.Services.Notifications;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly INotificationService? _notificationService;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<string> passwordHasher,
        INotificationService? notificationService = null,
        ILogger<AuthService>? logger = null)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CompanyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(request.CompanyId));
        }

        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
        {
            throw new ArgumentException("UserName or Email is required.", nameof(request.UserNameOrEmail));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        string trimmedIdentifier = request.UserNameOrEmail.Trim();

        var loginUser = await _userRepository.GetForLoginAsync(request.CompanyId, trimmedIdentifier);

        // Security requirement: if user does not exist or is soft-deleted, return 401 Unauthorized
        if (loginUser is null || loginUser.IsDeleted)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        // Inactive / unapproved user returns specific rejection
        if (!loginUser.IsActive)
        {
            throw new InvalidOperationException("Your account is pending administrator approval. Please wait until an administrator approves your registration.");
        }

        // Verify hashed password
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            loginUser.UserName, loginUser.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        var (token, expiresAt) = _jwtTokenService.GenerateToken(loginUser);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserResponseDto
            {
                GId = loginUser.GId,
                UserName = loginUser.UserName,
                FullName = loginUser.FullName,
                Email = loginUser.Email,
                RoleCode = loginUser.RoleCode,
                DepartmentGId = loginUser.DepartmentGId,
                DepartmentName = loginUser.DepartmentName,
                CompanyId = loginUser.CompanyId,
                IsActive = loginUser.IsActive
            }
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        int companyId = request.CompanyId > 0 ? request.CompanyId : 1;

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(request.FullName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(request.Password));
        }

        string trimmedEmail = request.Email.Trim().ToLowerInvariant();
        string userName = trimmedEmail.Split('@')[0];

        // Check for existing user
        var existing = await _userRepository.GetForLoginAsync(companyId, trimmedEmail);
        if (existing is not null)
        {
            throw new InvalidOperationException($"An account with email '{trimmedEmail}' already exists.");
        }

        string passwordHash = _passwordHasher.HashPassword(userName, request.Password);

        var newUser = new ResolveHub.Core.Entities.UserMaster
        {
            CompanyId = companyId,
            DepartmentGId = null,
            UserName = userName,
            FullName = request.FullName.Trim(),
            Email = trimmedEmail,
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordHash,
            RoleCode = "USER",
            IsActive = false, // Newly registered users are pending admin approval
            CreatedBy = 0
        };

        var created = await _userRepository.InsertAsync(newUser);
        if (created is null)
        {
            throw new InvalidOperationException("Failed to register user. Please try again.");
        }

        // Publish in-app notification to administrator that a new user registration requires approval
        if (_notificationService is not null)
        {
            try
            {
                var adminRecord = await _userRepository.GetForLoginAsync(companyId, "admin");
                if (adminRecord is not null)
                {
                    await _notificationService.PublishAsync(new NotificationEvent
                    {
                        EventType = NotificationEventTypes.UserAccountCreated,
                        CompanyId = companyId,
                        ActorUserId = created.Id,
                        RecipientUserIds = new[] { adminRecord.Id },
                        Title = "New Registration Awaiting Approval",
                        Message = $"Requester {created.FullName} ({created.Email}) registered and is awaiting administrator approval.",
                        ActionUrl = "/users",
                        QueueEmail = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to publish notification for new user registration.");
            }
        }

        return new LoginResponseDto
        {
            Token = string.Empty,
            ExpiresAt = DateTime.UtcNow,
            User = new UserResponseDto
            {
                GId = created.GId,
                UserName = created.UserName,
                FullName = created.FullName,
                Email = created.Email,
                RoleCode = created.RoleCode,
                DepartmentGId = created.DepartmentGId,
                DepartmentName = created.DepartmentName,
                CompanyId = created.CompanyId,
                IsActive = created.IsActive
            }
        };
    }
}
