namespace ResolveHub.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

public class UserService : IUserService
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADMIN", "AGENT", "USER"
    };

    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPasswordHasher<string> _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        IPasswordHasher<string> passwordHasher)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> CreateAsync(int companyId, int createdBy, CreateUserDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        if (createdBy <= 0)
        {
            throw new ArgumentException("Invalid user ID.", nameof(createdBy));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ArgumentException("UserName is required and cannot be empty.", nameof(request.UserName));
        }

        string trimmedUserName = request.UserName.Trim();
        if (trimmedUserName.Length > 100)
        {
            throw new ArgumentException("UserName cannot exceed 100 characters.", nameof(request.UserName));
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("FullName is required and cannot be empty.", nameof(request.FullName));
        }

        string trimmedFullName = request.FullName.Trim();
        if (trimmedFullName.Length > 150)
        {
            throw new ArgumentException("FullName cannot exceed 150 characters.", nameof(request.FullName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required and cannot be empty.", nameof(request.Email));
        }

        string trimmedEmail = request.Email.Trim();
        if (!IsValidEmail(trimmedEmail))
        {
            throw new ArgumentException("Invalid email address format.", nameof(request.Email));
        }

        if (trimmedEmail.Length > 150)
        {
            throw new ArgumentException("Email cannot exceed 150 characters.", nameof(request.Email));
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        if (trimmedPhone != null && trimmedPhone.Length > 30)
        {
            throw new ArgumentException("Phone cannot exceed 30 characters.", nameof(request.Phone));
        }

        if (string.IsNullOrWhiteSpace(request.RoleCode) || !AllowedRoles.Contains(request.RoleCode.Trim()))
        {
            throw new ArgumentException("RoleCode must be one of: ADMIN, AGENT, USER.", nameof(request.RoleCode));
        }

        string normalizedRole = request.RoleCode.Trim().ToUpperInvariant();

        ValidatePasswordPolicy(request.Password);

        // Validate department if supplied
        if (request.DepartmentGId.HasValue && request.DepartmentGId.Value != Guid.Empty)
        {
            var dept = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId.Value);
            if (dept is null)
            {
                throw new ArgumentException("The specified department does not exist for this company.", nameof(request.DepartmentGId));
            }
        }

        // Duplicate checks for UserName and Email
        var (isDupUserName, isDupEmail) = await _userRepository.CheckDuplicateAsync(companyId, trimmedUserName, trimmedEmail);
        if (isDupUserName)
        {
            throw new InvalidOperationException($"A user with username '{trimmedUserName}' already exists for this company.");
        }

        if (isDupEmail)
        {
            throw new InvalidOperationException($"A user with email '{trimmedEmail}' already exists for this company.");
        }

        // Hash password securely
        string passwordHash = _passwordHasher.HashPassword(trimmedUserName, request.Password);

        var userEntity = new UserMaster
        {
            CompanyId = companyId,
            DepartmentGId = (request.DepartmentGId.HasValue && request.DepartmentGId.Value != Guid.Empty) ? request.DepartmentGId : null,
            UserName = trimmedUserName,
            FullName = trimmedFullName,
            Email = trimmedEmail,
            Phone = trimmedPhone,
            PasswordHash = passwordHash,
            RoleCode = normalizedRole,
            IsActive = request.IsActive,
            CreatedBy = createdBy
        };

        var createdUser = await _userRepository.InsertAsync(userEntity);

        if (createdUser is null)
        {
            throw new InvalidOperationException("Failed to create the user record in database.");
        }

        return MapToResponseDto(createdUser);
    }

    public async Task<UserResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateUserDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        if (lastUpdBy <= 0)
        {
            throw new ArgumentException("Invalid user ID.", nameof(lastUpdBy));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ArgumentException("UserName is required and cannot be empty.", nameof(request.UserName));
        }

        string trimmedUserName = request.UserName.Trim();
        if (trimmedUserName.Length > 100)
        {
            throw new ArgumentException("UserName cannot exceed 100 characters.", nameof(request.UserName));
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("FullName is required and cannot be empty.", nameof(request.FullName));
        }

        string trimmedFullName = request.FullName.Trim();
        if (trimmedFullName.Length > 150)
        {
            throw new ArgumentException("FullName cannot exceed 150 characters.", nameof(request.FullName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required and cannot be empty.", nameof(request.Email));
        }

        string trimmedEmail = request.Email.Trim();
        if (!IsValidEmail(trimmedEmail))
        {
            throw new ArgumentException("Invalid email address format.", nameof(request.Email));
        }

        if (trimmedEmail.Length > 150)
        {
            throw new ArgumentException("Email cannot exceed 150 characters.", nameof(request.Email));
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        if (trimmedPhone != null && trimmedPhone.Length > 30)
        {
            throw new ArgumentException("Phone cannot exceed 30 characters.", nameof(request.Phone));
        }

        if (string.IsNullOrWhiteSpace(request.RoleCode) || !AllowedRoles.Contains(request.RoleCode.Trim()))
        {
            throw new ArgumentException("RoleCode must be one of: ADMIN, AGENT, USER.", nameof(request.RoleCode));
        }

        string normalizedRole = request.RoleCode.Trim().ToUpperInvariant();

        // Validate department if supplied
        if (request.DepartmentGId.HasValue && request.DepartmentGId.Value != Guid.Empty)
        {
            var dept = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId.Value);
            if (dept is null)
            {
                throw new ArgumentException("The specified department does not exist for this company.", nameof(request.DepartmentGId));
            }
        }

        // Duplicate checks excluding self
        var (isDupUserName, isDupEmail) = await _userRepository.CheckDuplicateAsync(companyId, trimmedUserName, trimmedEmail, gId);
        if (isDupUserName)
        {
            throw new InvalidOperationException($"Another user with username '{trimmedUserName}' already exists for this company.");
        }

        if (isDupEmail)
        {
            throw new InvalidOperationException($"Another user with email '{trimmedEmail}' already exists for this company.");
        }

        var userEntity = new UserMaster
        {
            CompanyId = companyId,
            GId = gId,
            DepartmentGId = (request.DepartmentGId.HasValue && request.DepartmentGId.Value != Guid.Empty) ? request.DepartmentGId : null,
            UserName = trimmedUserName,
            FullName = trimmedFullName,
            Email = trimmedEmail,
            Phone = trimmedPhone,
            RoleCode = normalizedRole,
            IsActive = request.IsActive,
            LastUpdBy = lastUpdBy
        };

        var updatedUser = await _userRepository.UpdateAsync(userEntity);

        if (updatedUser is null)
        {
            return null;
        }

        return MapToResponseDto(updatedUser);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        return await _userRepository.DeleteAsync(companyId, gId, lastUpdBy);
    }

    public async Task<UserResponseDto?> GetByGIdAsync(int companyId, Guid gId)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        var user = await _userRepository.GetByGIdAsync(companyId, gId);

        if (user is null)
        {
            return null;
        }

        return MapToResponseDto(user);
    }

    public async Task<PagedResult<UserResponseDto>> GetListingAsync(int companyId, UserListRequestDto request)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        request ??= new UserListRequestDto();

        var pagedUsers = await _userRepository.GetListingAsync(companyId, request);

        var responseDtos = pagedUsers.Items
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResult<UserResponseDto>
        {
            Items = responseDtos,
            TotalCount = pagedUsers.TotalCount,
            PageNumber = pagedUsers.PageNumber,
            PageSize = pagedUsers.PageSize
        };
    }

    public async Task<bool> ChangePasswordAsync(int companyId, Guid gId, int lastUpdBy, ChangePasswordDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        ValidatePasswordPolicy(request.NewPassword);

        var user = await _userRepository.GetByGIdAsync(companyId, gId);
        if (user is null)
        {
            return false;
        }

        string newPasswordHash = _passwordHasher.HashPassword(user.UserName, request.NewPassword);

        return await _userRepository.UpdatePasswordAsync(companyId, gId, newPasswordHash, lastUpdBy);
    }

    private static void ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required and cannot be empty.", nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));
        }

        if (!password.Any(char.IsUpper))
        {
            throw new ArgumentException("Password must contain at least one uppercase letter.", nameof(password));
        }

        if (!password.Any(char.IsLower))
        {
            throw new ArgumentException("Password must contain at least one lowercase letter.", nameof(password));
        }

        if (!password.Any(char.IsDigit))
        {
            throw new ArgumentException("Password must contain at least one number.", nameof(password));
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw new ArgumentException("Password must contain at least one special character.", nameof(password));
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static UserResponseDto MapToResponseDto(UserMaster user)
    {
        return new UserResponseDto
        {
            GId = user.GId,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            RoleCode = user.RoleCode,
            DepartmentGId = user.DepartmentGId,
            DepartmentName = user.DepartmentName,
            CompanyId = user.CompanyId,
            IsActive = user.IsActive,
            Created = user.Created,
            LastUpd = user.LastUpd
        };
    }
}
