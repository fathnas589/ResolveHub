namespace ResolveHub.Services.Implementations;

using System;
using System.Linq;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Categories;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IDepartmentRepository departmentRepository)
    {
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<CategoryResponseDto> CreateAsync(int companyId, int createdBy, CreateCategoryDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            throw new ArgumentException("Category name is required and cannot be empty.", nameof(request.CategoryName));
        }

        if (request.DepartmentGId == Guid.Empty)
        {
            throw new ArgumentException("Department is required.", nameof(request.DepartmentGId));
        }

        string trimmedName = request.CategoryName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Validate parent Department existence for this company
        var department = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId);
        if (department is null)
        {
            throw new ArgumentException("The specified department does not exist for this company.", nameof(request.DepartmentGId));
        }

        // Check for duplicate category name within this department
        bool isDuplicate = await _categoryRepository.CheckDuplicateNameAsync(companyId, request.DepartmentGId, trimmedName);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"A category with the name '{trimmedName}' already exists for this department.");
        }

        var categoryEntity = new Category
        {
            CompanyId = companyId,
            DepartmentGId = request.DepartmentGId,
            CategoryName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            CreatedBy = createdBy
        };

        var createdCategory = await _categoryRepository.InsertAsync(categoryEntity);

        if (createdCategory is null)
        {
            throw new InvalidOperationException("Failed to create the category record in database.");
        }

        return MapToResponseDto(createdCategory);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateCategoryDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            throw new ArgumentException("Category name is required and cannot be empty.", nameof(request.CategoryName));
        }

        if (request.DepartmentGId == Guid.Empty)
        {
            throw new ArgumentException("Department is required.", nameof(request.DepartmentGId));
        }

        string trimmedName = request.CategoryName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Validate parent Department existence for this company
        var department = await _departmentRepository.GetByGIdAsync(companyId, request.DepartmentGId);
        if (department is null)
        {
            throw new ArgumentException("The specified department does not exist for this company.", nameof(request.DepartmentGId));
        }

        // Check for duplicate category name within this department (excluding self)
        bool isDuplicate = await _categoryRepository.CheckDuplicateNameAsync(companyId, request.DepartmentGId, trimmedName, gId);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Another category with the name '{trimmedName}' already exists for this department.");
        }

        var categoryEntity = new Category
        {
            CompanyId = companyId,
            GId = gId,
            DepartmentGId = request.DepartmentGId,
            CategoryName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            LastUpdBy = lastUpdBy
        };

        var updatedCategory = await _categoryRepository.UpdateAsync(categoryEntity);

        if (updatedCategory is null)
        {
            return null;
        }

        return MapToResponseDto(updatedCategory);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        return await _categoryRepository.DeleteAsync(companyId, gId, lastUpdBy);
    }

    public async Task<CategoryResponseDto?> GetByGIdAsync(int companyId, Guid gId)
    {
        var category = await _categoryRepository.GetByGIdAsync(companyId, gId);

        if (category is null)
        {
            return null;
        }

        return MapToResponseDto(category);
    }

    public async Task<PagedResult<CategoryResponseDto>> GetListingAsync(int companyId, CategoryListRequestDto request)
    {
        request ??= new CategoryListRequestDto();

        var pagedCategories = await _categoryRepository.GetListingAsync(companyId, request);

        var responseDtos = pagedCategories.Items
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResult<CategoryResponseDto>
        {
            Items = responseDtos,
            TotalCount = pagedCategories.TotalCount,
            PageNumber = pagedCategories.PageNumber,
            PageSize = pagedCategories.PageSize
        };
    }

    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            GId = category.GId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            DepartmentGId = category.DepartmentGId,
            DepartmentName = category.DepartmentName,
            IsActive = category.IsActive,
            Created = category.Created,
            LastUpd = category.LastUpd
        };
    }
}
