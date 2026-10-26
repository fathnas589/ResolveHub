namespace ResolveHub.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.DTOs.Tickets;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;
using ResolveHub.Services.Notifications;

public class TicketService : ITicketService
{
    private static readonly HashSet<string> AllowedTicketTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "COMPLAINT",
        "SERVICE_REQUEST"
    };

    private static readonly HashSet<string> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        "LOW",
        "MEDIUM",
        "HIGH",
        "CRITICAL"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "OPEN",
        "ASSIGNED",
        "IN_PROGRESS",
        "PENDING_USER",
        "RESOLVED",
        "CLOSED",
        "REJECTED"
    };

    private readonly ITicketRepository _ticketRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISlaPolicyRepository _slaPolicyRepository;
    private readonly ITicketSlaRepository _ticketSlaRepository;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        IDepartmentRepository departmentRepository,
        ICategoryRepository categoryRepository,
        ISubCategoryRepository subCategoryRepository,
        IUserRepository userRepository,
        ISlaPolicyRepository slaPolicyRepository,
        ITicketSlaRepository ticketSlaRepository,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _userRepository = userRepository;
        _slaPolicyRepository = slaPolicyRepository;
        _ticketSlaRepository = ticketSlaRepository;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        // 1. Validate TicketType
        if (string.IsNullOrWhiteSpace(request.TicketType) || !AllowedTicketTypes.Contains(request.TicketType.Trim()))
        {
            throw new ArgumentException("Invalid ticket type. Allowed values are COMPLAINT, SERVICE_REQUEST.", nameof(request.TicketType));
        }
        string normalizedTicketType = request.TicketType.Trim().ToUpperInvariant();

        // 2. Validate Priority
        if (string.IsNullOrWhiteSpace(request.Priority) || !AllowedPriorities.Contains(request.Priority.Trim()))
        {
            throw new ArgumentException("Invalid priority. Allowed values are LOW, MEDIUM, HIGH, CRITICAL.", nameof(request.Priority));
        }
        string normalizedPriority = request.Priority.Trim().ToUpperInvariant();

        // 3. Validate Subject & Description
        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Subject is required and cannot be empty.", nameof(request.Subject));
        }
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Description is required and cannot be empty.", nameof(request.Description));
        }

        // 4. Validate Department
        var department = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId);
        if (department is null || !department.IsActive)
        {
            throw new ArgumentException("The specified department does not exist or is inactive.", nameof(request.DepartmentGId));
        }

        // 5. Validate Category and Department relationship
        var category = await _categoryRepository.GetByGIdAsync(companyId, request.CategoryGId);
        if (category is null || !category.IsActive || category.DepartmentGId != request.DepartmentGId)
        {
            throw new ArgumentException("The specified category does not exist, is inactive, or does not belong to the selected department.", nameof(request.CategoryGId));
        }

        // 6. Validate SubCategory if provided
        Guid? subCategoryGId = null;
        int? subCategoryId = null;
        if (request.SubCategoryGId.HasValue && request.SubCategoryGId.Value != Guid.Empty)
        {
            var subCategory = await _subCategoryRepository.GetByGIdAsync(companyId, request.SubCategoryGId.Value);
            if (subCategory is null || subCategory.CategoryGId != request.CategoryGId)
            {
                throw new ArgumentException("The specified sub-category does not exist or does not belong to the specified category.", nameof(request.SubCategoryGId));
            }
            subCategoryGId = request.SubCategoryGId.Value;
            subCategoryId = subCategory.Id;
        }

        var createdTicket = await _ticketRepository.InsertAsync(
            companyId,
            userId,
            request.DepartmentGId,
            request.CategoryGId,
            subCategoryGId,
            normalizedTicketType,
            request.Subject.Trim(),
            request.Description.Trim(),
            normalizedPriority);

        if (createdTicket is null)
        {
            throw new InvalidOperationException("Failed to create the ticket in the database.");
        }

        TicketSla? createdSla = null;

        // 7. Snapshot SLA policy matching this ticket
        try
        {
            var bestPolicy = await _slaPolicyRepository.GetBestMatchAsync(
                companyId,
                department.Id,
                category.Id,
                subCategoryId,
                normalizedPriority,
                normalizedTicketType);

            if (bestPolicy != null)
            {
                DateTime createdUtc = createdTicket.Created;
                var firstResponseDueAt = createdUtc.AddMinutes(bestPolicy.FirstResponseMinutes);
                var resolutionDueAt = createdUtc.AddMinutes(bestPolicy.ResolutionMinutes);
                DateTime? escalationDueAt = bestPolicy.EscalationBeforeMinutes > 0
                    ? resolutionDueAt.AddMinutes(-bestPolicy.EscalationBeforeMinutes)
                    : null;

                var ticketSla = new TicketSla
                {
                    TicketId = createdTicket.Id,
                    SlaPolicyId = bestPolicy.Id,
                    FirstResponseMinutes = bestPolicy.FirstResponseMinutes,
                    ResolutionMinutes = bestPolicy.ResolutionMinutes,
                    FirstResponseDueAt = firstResponseDueAt,
                    ResolutionDueAt = resolutionDueAt,
                    EscalationDueAt = escalationDueAt
                };

                createdSla = await _ticketSlaRepository.InsertAsync(ticketSla);
            }
            else
            {
                _logger.LogWarning("No matching SLA policy found for Ticket {TicketNumber} (Company {CompanyId}). Ticket created without SLA.", createdTicket.TicketNumber, companyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match/snapshot SLA for Ticket {TicketNumber}", createdTicket.TicketNumber);
        }

        var dto = MapToDto(createdTicket);
        if (createdSla != null)
        {
            dto.Sla = MapToSlaSummary(createdSla, dto.Status);
        }

        try
        {
            _ = _notificationService.PublishTicketCreatedAsync(
                companyId,
                createdTicket.Id,
                createdTicket.GId,
                createdTicket.TicketNumber,
                createdTicket.Subject,
                userId,
                _currentUserService.FullName,
                createdTicket.AssignedToUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ticket created notification for Ticket {TicketNumber}", createdTicket.TicketNumber);
        }

        return dto;
    }

    public async Task<TicketResponseDto> GetTicketByGIdAsync(Guid gId)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found or access is restricted.");
        }

        var dto = MapToDto(ticket);
        var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, ticket.Id);
        if (sla != null)
        {
            dto.Sla = MapToSlaSummary(sla, dto.Status);
        }

        return dto;
    }

    public async Task<PagedResult<TicketResponseDto>> GetTicketsListingAsync(TicketListRequestDto request)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        var pagedResult = await _ticketRepository.GetListingAsync(companyId, userId, role, request);

        var dtos = new List<TicketResponseDto>(pagedResult.Items.Count);
        foreach (var item in pagedResult.Items)
        {
            var dto = MapToDto(item);
            var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, item.Id);
            if (sla != null)
            {
                dto.Sla = MapToSlaSummary(sla, dto.Status);
            }
            dtos.Add(dto);
        }

        return new PagedResult<TicketResponseDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<TicketResponseDto> UpdateTicketAsync(Guid gId, UpdateTicketDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        var existingTicket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (existingTicket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found or access is restricted.");
        }

        // Role authorization check for editing
        if (role == "USER" && existingTicket.Status != "OPEN")
        {
            throw new InvalidOperationException("Tickets that are no longer OPEN cannot be edited by the submitter.");
        }

        // Validate Priority
        if (string.IsNullOrWhiteSpace(request.Priority) || !AllowedPriorities.Contains(request.Priority.Trim()))
        {
            throw new ArgumentException("Invalid priority. Allowed values are LOW, MEDIUM, HIGH, CRITICAL.", nameof(request.Priority));
        }
        string normalizedPriority = request.Priority.Trim().ToUpperInvariant();

        // Validate Subject & Description
        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Subject is required and cannot be empty.", nameof(request.Subject));
        }
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Description is required and cannot be empty.", nameof(request.Description));
        }

        // Validate Department
        var department = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId);
        if (department is null || !department.IsActive)
        {
            throw new ArgumentException("The specified department does not exist or is inactive.", nameof(request.DepartmentGId));
        }

        // Validate Category
        var category = await _categoryRepository.GetByGIdAsync(companyId, request.CategoryGId);
        if (category is null || !category.IsActive || category.DepartmentGId != request.DepartmentGId)
        {
            throw new ArgumentException("The specified category does not exist, is inactive, or does not belong to the selected department.", nameof(request.CategoryGId));
        }

        // Validate SubCategory if provided
        Guid? subCategoryGId = null;
        if (request.SubCategoryGId.HasValue && request.SubCategoryGId.Value != Guid.Empty)
        {
            var subCategory = await _subCategoryRepository.GetByGIdAsync(companyId, request.SubCategoryGId.Value);
            if (subCategory is null || subCategory.CategoryGId != request.CategoryGId)
            {
                throw new ArgumentException("The specified sub-category does not exist or does not belong to the specified category.", nameof(request.SubCategoryGId));
            }
            subCategoryGId = request.SubCategoryGId.Value;
        }

        var updated = await _ticketRepository.UpdateAsync(
            companyId,
            gId,
            request.DepartmentGId,
            request.CategoryGId,
            subCategoryGId,
            request.Subject.Trim(),
            request.Description.Trim(),
            normalizedPriority,
            userId);

        if (updated is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        var dto = MapToDto(updated);
        var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, updated.Id);
        if (sla != null)
        {
            dto.Sla = MapToSlaSummary(sla, dto.Status);
        }

        return dto;
    }

    public async Task<TicketResponseDto> AssignTicketAsync(Guid gId, AssignTicketDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        if (role != "ADMIN" && role != "AGENT")
        {
            throw new UnauthorizedAccessException("Only ADMIN or AGENT roles are authorized to assign tickets.");
        }

        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        // Validate target user
        var targetUser = await _userRepository.GetByGIdAsync(companyId, request.AssignedToUserGId);
        if (targetUser is null || !targetUser.IsActive || targetUser.RoleCode != "AGENT" && targetUser.RoleCode != "ADMIN")
        {
            throw new ArgumentException("The target user is not eligible for ticket assignment.", nameof(request.AssignedToUserGId));
        }

        // Department compatibility policy:
        // If caller is AGENT, and target agent has a department, it must match ticket department
        if (role == "AGENT" && targetUser.DepartmentGId.HasValue && targetUser.DepartmentGId.Value != ticket.DepartmentGId)
        {
            throw new InvalidOperationException("Agents cannot assign tickets to another department.");
        }

        var assigned = await _ticketRepository.AssignAsync(companyId, gId, request.AssignedToUserGId, userId);
        if (assigned is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        var dto = MapToDto(assigned);
        var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, assigned.Id);
        if (sla != null)
        {
            dto.Sla = MapToSlaSummary(sla, dto.Status);
        }

        try
        {
            bool isReassignment = ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value != targetUser.Id;
            _ = _notificationService.PublishTicketAssignedAsync(
                companyId,
                assigned.Id,
                assigned.GId,
                assigned.TicketNumber,
                assigned.Subject,
                targetUser.Id,
                userId,
                _currentUserService.FullName,
                isReassignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ticket assignment notification for Ticket {TicketNumber}", assigned.TicketNumber);
        }

        return dto;
    }

    public async Task<TicketResponseDto> UpdateTicketStatusAsync(Guid gId, UpdateTicketStatusDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        if (string.IsNullOrWhiteSpace(request.Status) || !AllowedStatuses.Contains(request.Status.Trim()))
        {
            throw new ArgumentException($"Invalid status '{request.Status}'.", nameof(request.Status));
        }
        string newStatus = request.Status.Trim().ToUpperInvariant();

        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        string currentStatus = ticket.Status.ToUpperInvariant();

        // Validate state transitions
        ValidateStatusTransition(currentStatus, newStatus, role);

        var updated = await _ticketRepository.UpdateStatusAsync(companyId, gId, newStatus, request.Comments, userId);
        if (updated is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        // Handle SLA Resolution or Reopen
        DateTime nowUtc = DateTime.UtcNow;
        if (newStatus == "RESOLVED")
        {
            try
            {
                await _ticketSlaRepository.UpdateResolutionAsync(ticket.Id, "RESOLVED", nowUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SLA resolution for Ticket {TicketId}", ticket.Id);
            }
        }
        else if (currentStatus == "RESOLVED" && newStatus == "IN_PROGRESS")
        {
            try
            {
                await _ticketSlaRepository.UpdateResolutionAsync(ticket.Id, "REOPENED", nowUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting SLA resolution on reopen for Ticket {TicketId}", ticket.Id);
            }
        }

        var dto = MapToDto(updated);
        var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, updated.Id);
        if (sla != null)
        {
            dto.Sla = MapToSlaSummary(sla, dto.Status);
        }

        try
        {
            _ = _notificationService.PublishTicketStatusChangedAsync(
                companyId,
                ticket.Id,
                ticket.GId,
                ticket.TicketNumber,
                ticket.Subject,
                currentStatus,
                newStatus,
                userId,
                _currentUserService.FullName,
                ticket.CreatedByUserId,
                ticket.AssignedToUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ticket status changed notification for Ticket {TicketNumber}", ticket.TicketNumber);
        }

        return dto;
    }

    public async Task DeleteTicketAsync(Guid gId)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        if (role != "ADMIN")
        {
            throw new UnauthorizedAccessException("Only ADMIN users can delete tickets.");
        }

        bool deleted = await _ticketRepository.DeleteAsync(companyId, gId, userId);
        if (!deleted)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }
    }

    public async Task<TicketCommentResponseDto> AddCommentAsync(Guid gId, CreateTicketCommentDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        if (string.IsNullOrWhiteSpace(request.CommentText))
        {
            throw new ArgumentException("Comment text cannot be empty.", nameof(request.CommentText));
        }

        // Security rule: USER cannot create internal comment
        if (request.IsInternal && role == "USER")
        {
            throw new UnauthorizedAccessException("Users with role USER are not authorized to create internal comments.");
        }

        // Verify ticket access
        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        var comment = await _ticketRepository.InsertCommentAsync(
            companyId,
            gId,
            request.CommentText.Trim(),
            request.IsInternal,
            userId);

        if (comment is null)
        {
            throw new InvalidOperationException("Failed to add comment.");
        }

        // If public comment by ADMIN or AGENT, satisfy first response SLA
        if (!request.IsInternal && (role == "ADMIN" || role == "AGENT"))
        {
            try
            {
                await _ticketSlaRepository.UpdateFirstResponseAsync(ticket.Id, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording first response SLA for Ticket {TicketId}", ticket.Id);
            }
        }

        if (!request.IsInternal)
        {
            try
            {
                bool isAuthorStaff = role == "ADMIN" || role == "AGENT";
                _ = _notificationService.PublishTicketCommentAddedAsync(
                    companyId,
                    ticket.Id,
                    ticket.GId,
                    ticket.TicketNumber,
                    ticket.Subject,
                    userId,
                    _currentUserService.FullName,
                    isAuthorStaff,
                    ticket.CreatedByUserId,
                    ticket.AssignedToUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish ticket comment notification for Ticket {TicketNumber}", ticket.TicketNumber);
            }
        }

        return new TicketCommentResponseDto
        {
            GId = comment.GId,
            CommentText = comment.CommentText,
            IsInternal = comment.IsInternal,
            Created = comment.Created,
            AuthorUserGId = comment.AuthorUserGId,
            AuthorUserName = comment.AuthorUserName,
            AuthorFullName = comment.AuthorFullName
        };
    }

    public async Task<IEnumerable<TicketCommentResponseDto>> GetCommentsAsync(Guid gId)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        // Verify ticket access
        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        var comments = await _ticketRepository.GetCommentsAsync(companyId, gId, userId, role);

        return comments.Select(c => new TicketCommentResponseDto
        {
            GId = c.GId,
            CommentText = c.CommentText,
            IsInternal = c.IsInternal,
            Created = c.Created,
            AuthorUserGId = c.AuthorUserGId,
            AuthorUserName = c.AuthorUserName,
            AuthorFullName = c.AuthorFullName
        });
    }

    public async Task<IEnumerable<TicketHistoryResponseDto>> GetHistoryAsync(Guid gId)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        // Verify ticket access
        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found.");
        }

        var history = await _ticketRepository.GetHistoryAsync(companyId, gId, userId, role);

        return history.Select(h => new TicketHistoryResponseDto
        {
            GId = h.GId,
            ActionType = h.ActionType,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            Comments = h.Comments,
            Created = h.Created,
            ActorUserGId = h.ActorUserGId,
            ActorUserName = h.ActorUserName,
            ActorFullName = h.ActorFullName
        });
    }

    public async Task<TicketSlaResponseDto?> GetTicketSlaAsync(Guid gId)
    {
        EnsureAuthenticated();
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        var ticket = await _ticketRepository.GetByGIdAsync(companyId, gId, userId, role);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with ID '{gId}' was not found or access is restricted.");
        }

        var sla = await _ticketSlaRepository.GetByTicketIdAsync(companyId, ticket.Id);
        if (sla is null)
        {
            return null;
        }

        return MapToSlaResponseDto(sla, ticket.Status);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.CompanyId <= 0 || _currentUserService.UserId <= 0)
        {
            throw new UnauthorizedAccessException("User is not authenticated or missing required tenant claims.");
        }
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus, string role)
    {
        if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return; // No-op transition
        }

        bool isAllowed = (currentStatus, newStatus) switch
        {
            ("OPEN", "ASSIGNED") => role is "ADMIN" or "AGENT",
            ("OPEN", "REJECTED") => role is "ADMIN" or "AGENT",

            ("ASSIGNED", "IN_PROGRESS") => role is "ADMIN" or "AGENT",
            ("ASSIGNED", "REJECTED") => role is "ADMIN" or "AGENT",
            ("ASSIGNED", "ASSIGNED") => role is "ADMIN" or "AGENT",

            ("IN_PROGRESS", "PENDING_USER") => role is "ADMIN" or "AGENT",
            ("IN_PROGRESS", "RESOLVED") => role is "ADMIN" or "AGENT",

            ("PENDING_USER", "IN_PROGRESS") => true, // Both user and agent can move back to in_progress
            ("PENDING_USER", "RESOLVED") => role is "ADMIN" or "AGENT",

            ("RESOLVED", "CLOSED") => true,
            ("RESOLVED", "IN_PROGRESS") => true, // Reopen

            ("CLOSED", "OPEN") => role is "ADMIN",
            ("REJECTED", "OPEN") => role is "ADMIN",

            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException($"Invalid status transition from '{currentStatus}' to '{newStatus}'.");
        }
    }

    private static TicketResponseDto MapToDto(Ticket ticket)
    {
        return new TicketResponseDto
        {
            GId = ticket.GId,
            TicketNumber = ticket.TicketNumber,
            TicketType = ticket.TicketType,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            DepartmentGId = ticket.DepartmentGId,
            DepartmentName = ticket.DepartmentName,
            CategoryGId = ticket.CategoryGId,
            CategoryName = ticket.CategoryName,
            SubCategoryGId = ticket.SubCategoryGId,
            SubCategoryName = ticket.SubCategoryName,
            CreatedByUserGId = ticket.CreatedByUserGId,
            CreatedByUserName = ticket.CreatedByUserName,
            CreatedByFullName = ticket.CreatedByFullName,
            AssignedToUserGId = ticket.AssignedToUserGId,
            AssignedToUserName = ticket.AssignedToUserName,
            AssignedToFullName = ticket.AssignedToFullName,
            Created = ticket.Created,
            LastUpd = ticket.LastUpd,
            ClosedAt = ticket.ClosedAt
        };
    }

    public static string DeriveSlaHealth(TicketSla? sla, string ticketStatus)
    {
        if (sla is null)
        {
            return "NO_SLA";
        }

        bool isResolvedOrClosed = ticketStatus is "RESOLVED" or "CLOSED";
        if (isResolvedOrClosed || sla.ResolvedAt.HasValue)
        {
            return sla.ResolutionBreached ? "COMPLETED_BREACHED" : "COMPLETED_ON_TIME";
        }

        DateTime nowUtc = DateTime.UtcNow;

        if (sla.ResolutionBreached || nowUtc > sla.ResolutionDueAt)
        {
            return "RESOLUTION_BREACHED";
        }

        if (!sla.FirstRespondedAt.HasValue && (sla.FirstResponseBreached || nowUtc > sla.FirstResponseDueAt))
        {
            return "FIRST_RESPONSE_BREACHED";
        }

        if (sla.EscalationDueAt.HasValue && nowUtc >= sla.EscalationDueAt.Value)
        {
            return "WARNING";
        }

        return "ON_TRACK";
    }

    private static TicketSlaSummaryDto MapToSlaSummary(TicketSla sla, string ticketStatus)
    {
        DateTime nowUtc = DateTime.UtcNow;
        string health = DeriveSlaHealth(sla, ticketStatus);

        long? firstRespSecs = sla.FirstRespondedAt.HasValue
            ? null
            : (long)(sla.FirstResponseDueAt - nowUtc).TotalSeconds;

        long? resolSecs = sla.ResolvedAt.HasValue
            ? null
            : (long)(sla.ResolutionDueAt - nowUtc).TotalSeconds;

        return new TicketSlaSummaryDto
        {
            SlaPolicyName = sla.SlaPolicyName,
            SlaHealth = health,
            FirstResponseDueAt = sla.FirstResponseDueAt,
            ResolutionDueAt = sla.ResolutionDueAt,
            FirstResponseBreached = sla.FirstResponseBreached,
            ResolutionBreached = sla.ResolutionBreached,
            FirstResponseRemainingSeconds = firstRespSecs,
            ResolutionRemainingSeconds = resolSecs
        };
    }

    private static TicketSlaResponseDto MapToSlaResponseDto(TicketSla sla, string ticketStatus)
    {
        DateTime nowUtc = DateTime.UtcNow;
        string health = DeriveSlaHealth(sla, ticketStatus);

        long? firstRespSecs = sla.FirstRespondedAt.HasValue
            ? null
            : (long)(sla.FirstResponseDueAt - nowUtc).TotalSeconds;

        long? resolSecs = sla.ResolvedAt.HasValue
            ? null
            : (long)(sla.ResolutionDueAt - nowUtc).TotalSeconds;

        return new TicketSlaResponseDto
        {
            GId = sla.GId,
            SlaPolicyGId = sla.SlaPolicyGId,
            SlaPolicyName = sla.SlaPolicyName,
            FirstResponseMinutes = sla.FirstResponseMinutes,
            ResolutionMinutes = sla.ResolutionMinutes,
            FirstResponseDueAt = sla.FirstResponseDueAt,
            FirstRespondedAt = sla.FirstRespondedAt,
            FirstResponseBreached = sla.FirstResponseBreached,
            ResolutionDueAt = sla.ResolutionDueAt,
            ResolvedAt = sla.ResolvedAt,
            ResolutionBreached = sla.ResolutionBreached,
            EscalationDueAt = sla.EscalationDueAt,
            EscalationTriggeredAt = sla.EscalationTriggeredAt,
            SlaHealth = health,
            FirstResponseRemainingSeconds = firstRespSecs,
            ResolutionRemainingSeconds = resolSecs,
            Created = sla.Created,
            LastUpd = sla.LastUpd
        };
    }
}
