namespace ResolveHub.Services.Implementations;

using System;
using System.Linq;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

public class SlaPolicyService : ISlaPolicyService
{
    private readonly ISlaPolicyRepository _slaPolicyRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISubCategoryRepository _subCategoryRepository;

    private static readonly string[] AllowedPriorities = { "LOW", "MEDIUM", "HIGH", "CRITICAL" };
    private static readonly string[] AllowedTicketTypes = { "COMPLAINT", "SERVICE_REQUEST" };

    public SlaPolicyService(
        ISlaPolicyRepository slaPolicyRepository,
        IDepartmentRepository departmentRepository,
        ICategoryRepository categoryRepository,
        ISubCategoryRepository subCategoryRepository)
    {
        _slaPolicyRepository = slaPolicyRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<SlaPolicyResponseDto> CreateAsync(int companyId, int createdBy, CreateSlaPolicyDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidatePolicyFields(request.Name, request.FirstResponseMinutes, request.ResolutionMinutes, request.EscalationBeforeMinutes, request.Priority, request.TicketType);

        await ValidateHierarchyAsync(companyId, request.DepartmentGId, request.CategoryGId, request.SubCategoryGId);

        string trimmedName = request.Name.Trim();
        bool isDuplicate = await _slaPolicyRepository.CheckDuplicateNameAsync(companyId, trimmedName);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"An SLA policy with the name '{trimmedName}' already exists for this company.");
        }

        var policyEntity = new SlaPolicy
        {
            CompanyId = companyId,
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DepartmentGId = request.DepartmentGId,
            CategoryGId = request.CategoryGId,
            SubCategoryGId = request.SubCategoryGId,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? null : request.Priority.Trim().ToUpperInvariant(),
            TicketType = string.IsNullOrWhiteSpace(request.TicketType) ? null : request.TicketType.Trim().ToUpperInvariant(),
            FirstResponseMinutes = request.FirstResponseMinutes,
            ResolutionMinutes = request.ResolutionMinutes,
            EscalationBeforeMinutes = request.EscalationBeforeMinutes,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            CreatedBy = createdBy
        };

        var createdPolicy = await _slaPolicyRepository.InsertAsync(policyEntity);
        if (createdPolicy is null)
        {
            throw new InvalidOperationException("Failed to create SLA policy.");
        }

        return MapToResponseDto(createdPolicy);
    }

    public async Task<SlaPolicyResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateSlaPolicyDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidatePolicyFields(request.Name, request.FirstResponseMinutes, request.ResolutionMinutes, request.EscalationBeforeMinutes, request.Priority, request.TicketType);

        var existingPolicy = await _slaPolicyRepository.GetByGIdAsync(companyId, gId);
        if (existingPolicy is null)
        {
            return null;
        }

        await ValidateHierarchyAsync(companyId, request.DepartmentGId, request.CategoryGId, request.SubCategoryGId);

        string trimmedName = request.Name.Trim();
        bool isDuplicate = await _slaPolicyRepository.CheckDuplicateNameAsync(companyId, trimmedName, gId);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"An SLA policy with the name '{trimmedName}' already exists for this company.");
        }

        var policyEntity = new SlaPolicy
        {
            CompanyId = companyId,
            GId = gId,
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DepartmentGId = request.DepartmentGId,
            CategoryGId = request.CategoryGId,
            SubCategoryGId = request.SubCategoryGId,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? null : request.Priority.Trim().ToUpperInvariant(),
            TicketType = string.IsNullOrWhiteSpace(request.TicketType) ? null : request.TicketType.Trim().ToUpperInvariant(),
            FirstResponseMinutes = request.FirstResponseMinutes,
            ResolutionMinutes = request.ResolutionMinutes,
            EscalationBeforeMinutes = request.EscalationBeforeMinutes,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            LastUpdBy = lastUpdBy
        };

        var updatedPolicy = await _slaPolicyRepository.UpdateAsync(policyEntity);
        if (updatedPolicy is null)
        {
            return null;
        }

        return MapToResponseDto(updatedPolicy);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        var existingPolicy = await _slaPolicyRepository.GetByGIdAsync(companyId, gId);
        if (existingPolicy is null)
        {
            return false;
        }

        return await _slaPolicyRepository.DeleteAsync(companyId, gId, lastUpdBy);
    }

    public async Task<SlaPolicyResponseDto?> GetByGIdAsync(int companyId, Guid gId)
    {
        var policy = await _slaPolicyRepository.GetByGIdAsync(companyId, gId);
        return policy is null ? null : MapToResponseDto(policy);
    }

    public async Task<PagedResult<SlaPolicyResponseDto>> GetListingAsync(int companyId, SlaPolicyListRequestDto request)
    {
        request ??= new SlaPolicyListRequestDto();

        var pagedPolicies = await _slaPolicyRepository.GetListingAsync(companyId, request);

        var responseDtos = pagedPolicies.Items
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResult<SlaPolicyResponseDto>
        {
            Items = responseDtos,
            TotalCount = pagedPolicies.TotalCount,
            PageNumber = pagedPolicies.PageNumber,
            PageSize = pagedPolicies.PageSize
        };
    }

    private static void ValidatePolicyFields(string name, int firstResponseMinutes, int resolutionMinutes, int escalationBeforeMinutes, string? priority, string? ticketType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("SLA policy name is required and cannot be empty.", nameof(name));
        }

        if (firstResponseMinutes <= 0)
        {
            throw new ArgumentException("First response minutes must be greater than zero.", nameof(firstResponseMinutes));
        }

        if (resolutionMinutes <= 0)
        {
            throw new ArgumentException("Resolution minutes must be greater than zero.", nameof(resolutionMinutes));
        }

        if (escalationBeforeMinutes < 0 || escalationBeforeMinutes >= resolutionMinutes)
        {
            throw new ArgumentException("Escalation before minutes must be non-negative and less than resolution minutes.", nameof(escalationBeforeMinutes));
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            string upperPriority = priority.Trim().ToUpperInvariant();
            if (!AllowedPriorities.Contains(upperPriority))
            {
                throw new ArgumentException($"Invalid priority '{priority}'. Allowed: LOW, MEDIUM, HIGH, CRITICAL.", nameof(priority));
            }
        }

        if (!string.IsNullOrWhiteSpace(ticketType))
        {
            string upperType = ticketType.Trim().ToUpperInvariant();
            if (!AllowedTicketTypes.Contains(upperType))
            {
                throw new ArgumentException($"Invalid ticket type '{ticketType}'. Allowed: COMPLAINT, SERVICE_REQUEST.", nameof(ticketType));
            }
        }
    }

    private async Task ValidateHierarchyAsync(int companyId, Guid? departmentGId, Guid? categoryGId, Guid? subCategoryGId)
    {
        Department? department = null;
        if (departmentGId.HasValue && departmentGId.Value != Guid.Empty)
        {
            department = await _departmentRepository.GetByGIdAsync(companyId, departmentGId.Value);
            if (department is null)
            {
                throw new ArgumentException("The specified department does not exist for this company.", nameof(departmentGId));
            }
        }

        Category? category = null;
        if (categoryGId.HasValue && categoryGId.Value != Guid.Empty)
        {
            category = await _categoryRepository.GetByGIdAsync(companyId, categoryGId.Value);
            if (category is null)
            {
                throw new ArgumentException("The specified category does not exist for this company.", nameof(categoryGId));
            }

            if (department != null && category.DepartmentGId != department.GId)
            {
                throw new ArgumentException("The specified category does not belong to the selected department.", nameof(categoryGId));
            }
        }

        if (subCategoryGId.HasValue && subCategoryGId.Value != Guid.Empty)
        {
            var subCategory = await _subCategoryRepository.GetByGIdAsync(companyId, subCategoryGId.Value);
            if (subCategory is null)
            {
                throw new ArgumentException("The specified subcategory does not exist for this company.", nameof(subCategoryGId));
            }

            if (category != null && subCategory.CategoryGId != category.GId)
            {
                throw new ArgumentException("The specified subcategory does not belong to the selected category.", nameof(subCategoryGId));
            }
        }
    }

    private static SlaPolicyResponseDto MapToResponseDto(SlaPolicy policy)
    {
        return new SlaPolicyResponseDto
        {
            GId = policy.GId,
            Name = policy.Name,
            Description = policy.Description,
            DepartmentGId = policy.DepartmentGId,
            DepartmentName = policy.DepartmentName,
            CategoryGId = policy.CategoryGId,
            CategoryName = policy.CategoryName,
            SubCategoryGId = policy.SubCategoryGId,
            SubCategoryName = policy.SubCategoryName,
            Priority = policy.Priority,
            TicketType = policy.TicketType,
            FirstResponseMinutes = policy.FirstResponseMinutes,
            ResolutionMinutes = policy.ResolutionMinutes,
            EscalationBeforeMinutes = policy.EscalationBeforeMinutes,
            IsDefault = policy.IsDefault,
            IsActive = policy.IsActive,
            Created = policy.Created,
            CreatedByName = policy.CreatedByName,
            LastUpd = policy.LastUpd,
            LastUpdByName = policy.LastUpdByName
        };
    }
}
