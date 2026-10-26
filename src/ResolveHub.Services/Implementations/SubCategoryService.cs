namespace ResolveHub.Services.Implementations;

using System;
using System.Linq;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.SubCategories;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

public class SubCategoryService : ISubCategoryService
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly ICategoryRepository _categoryRepository;

    public SubCategoryService(
        ISubCategoryRepository subCategoryRepository,
        ICategoryRepository categoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<SubCategoryResponseDto> CreateAsync(int companyId, int createdBy, CreateSubCategoryDto request)
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

        if (string.IsNullOrWhiteSpace(request.SubCategoryName))
        {
            throw new ArgumentException("SubCategory name is required and cannot be empty.", nameof(request.SubCategoryName));
        }

        string trimmedName = request.SubCategoryName.Trim();
        if (trimmedName.Length > 100)
        {
            throw new ArgumentException("SubCategory name cannot exceed 100 characters.", nameof(request.SubCategoryName));
        }

        if (request.CategoryGId == Guid.Empty)
        {
            throw new ArgumentException("Category is required.", nameof(request.CategoryGId));
        }

        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (trimmedDescription != null && trimmedDescription.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(request.Description));
        }

        // Validate parent Category existence for this company and ensure not deleted
        var category = await _categoryRepository.GetByGIdAsync(companyId, request.CategoryGId);
        if (category is null)
        {
            throw new ArgumentException("The specified category does not exist for this company.", nameof(request.CategoryGId));
        }

        // Check for duplicate subcategory name within this category
        bool isDuplicate = await _subCategoryRepository.CheckDuplicateNameAsync(companyId, request.CategoryGId, trimmedName);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"A subcategory with the name '{trimmedName}' already exists for this category.");
        }

        var subCategoryEntity = new SubCategory
        {
            CompanyId = companyId,
            CategoryGId = request.CategoryGId,
            SubCategoryName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            CreatedBy = createdBy
        };

        var createdSubCategory = await _subCategoryRepository.InsertAsync(subCategoryEntity);

        if (createdSubCategory is null)
        {
            throw new InvalidOperationException("Failed to create the subcategory record in database.");
        }

        return MapToResponseDto(createdSubCategory);
    }

    public async Task<SubCategoryResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateSubCategoryDto request)
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

        if (string.IsNullOrWhiteSpace(request.SubCategoryName))
        {
            throw new ArgumentException("SubCategory name is required and cannot be empty.", nameof(request.SubCategoryName));
        }

        string trimmedName = request.SubCategoryName.Trim();
        if (trimmedName.Length > 100)
        {
            throw new ArgumentException("SubCategory name cannot exceed 100 characters.", nameof(request.SubCategoryName));
        }

        if (request.CategoryGId == Guid.Empty)
        {
            throw new ArgumentException("Category is required.", nameof(request.CategoryGId));
        }

        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (trimmedDescription != null && trimmedDescription.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(request.Description));
        }

        // Validate parent Category existence for this company
        var category = await _categoryRepository.GetByGIdAsync(companyId, request.CategoryGId);
        if (category is null)
        {
            throw new ArgumentException("The specified category does not exist for this company.", nameof(request.CategoryGId));
        }

        // Check for duplicate subcategory name within this category (excluding self)
        bool isDuplicate = await _subCategoryRepository.CheckDuplicateNameAsync(companyId, request.CategoryGId, trimmedName, gId);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Another subcategory with the name '{trimmedName}' already exists for this category.");
        }

        var subCategoryEntity = new SubCategory
        {
            CompanyId = companyId,
            GId = gId,
            CategoryGId = request.CategoryGId,
            SubCategoryName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            LastUpdBy = lastUpdBy
        };

        var updatedSubCategory = await _subCategoryRepository.UpdateAsync(subCategoryEntity);

        if (updatedSubCategory is null)
        {
            return null;
        }

        return MapToResponseDto(updatedSubCategory);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        return await _subCategoryRepository.DeleteAsync(companyId, gId, lastUpdBy);
    }

    public async Task<SubCategoryResponseDto?> GetByGIdAsync(int companyId, Guid gId)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        var subCategory = await _subCategoryRepository.GetByGIdAsync(companyId, gId);

        if (subCategory is null)
        {
            return null;
        }

        return MapToResponseDto(subCategory);
    }

    public async Task<PagedResult<SubCategoryResponseDto>> GetListingAsync(int companyId, SubCategoryListRequestDto request)
    {
        if (companyId <= 0)
        {
            throw new ArgumentException("Invalid company ID.", nameof(companyId));
        }

        request ??= new SubCategoryListRequestDto();

        var pagedSubCategories = await _subCategoryRepository.GetListingAsync(companyId, request);

        var responseDtos = pagedSubCategories.Items
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResult<SubCategoryResponseDto>
        {
            Items = responseDtos,
            TotalCount = pagedSubCategories.TotalCount,
            PageNumber = pagedSubCategories.PageNumber,
            PageSize = pagedSubCategories.PageSize
        };
    }

    private static SubCategoryResponseDto MapToResponseDto(SubCategory subCategory)
    {
        return new SubCategoryResponseDto
        {
            GId = subCategory.GId,
            SubCategoryName = subCategory.SubCategoryName,
            Description = subCategory.Description,
            CategoryGId = subCategory.CategoryGId,
            CategoryName = subCategory.CategoryName,
            DepartmentGId = subCategory.DepartmentGId,
            DepartmentName = subCategory.DepartmentName,
            CompanyId = subCategory.CompanyId,
            IsActive = subCategory.IsActive,
            Created = subCategory.Created,
            LastUpd = subCategory.LastUpd
        };
    }
}
