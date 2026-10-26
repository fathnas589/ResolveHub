namespace ResolveHub.Services.Implementations;

using System;
using System.Linq;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Departments;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<DepartmentResponseDto> CreateAsync(int companyId, int createdBy, CreateDepartmentDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.DepartmentName))
        {
            throw new ArgumentException("Department name is required and cannot be empty.", nameof(request.DepartmentName));
        }

        string trimmedName = request.DepartmentName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Check for existing duplicate department name in the company
        bool isDuplicate = await _departmentRepository.CheckDuplicateNameAsync(companyId, trimmedName);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"A department with the name '{trimmedName}' already exists for this company.");
        }

        var departmentEntity = new Department
        {
            CompanyId = companyId,
            DepartmentName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            CreatedBy = createdBy
        };

        var createdDepartment = await _departmentRepository.InsertAsync(departmentEntity);

        if (createdDepartment is null)
        {
            throw new InvalidOperationException("Failed to create the department record in database.");
        }

        return MapToResponseDto(createdDepartment);
    }

    public async Task<DepartmentResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateDepartmentDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.DepartmentName))
        {
            throw new ArgumentException("Department name is required and cannot be empty.", nameof(request.DepartmentName));
        }

        string trimmedName = request.DepartmentName.Trim();
        string? trimmedDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Check for duplicate name excluding current department
        bool isDuplicate = await _departmentRepository.CheckDuplicateNameAsync(companyId, trimmedName, gId);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Another department with the name '{trimmedName}' already exists for this company.");
        }

        var departmentEntity = new Department
        {
            CompanyId = companyId,
            GId = gId,
            DepartmentName = trimmedName,
            Description = trimmedDescription,
            IsActive = request.IsActive,
            LastUpdBy = lastUpdBy
        };

        var updatedDepartment = await _departmentRepository.UpdateAsync(departmentEntity);

        if (updatedDepartment is null)
        {
            return null;
        }

        return MapToResponseDto(updatedDepartment);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        return await _departmentRepository.DeleteAsync(companyId, gId, lastUpdBy);
    }

    public async Task<DepartmentResponseDto?> GetByGIdAsync(int companyId, Guid gId)
    {
        var department = await _departmentRepository.GetByGIdAsync(companyId, gId);

        if (department is null)
        {
            return null;
        }

        return MapToResponseDto(department);
    }

    public async Task<PagedResult<DepartmentResponseDto>> GetListingAsync(int companyId, DepartmentListRequestDto request)
    {
        request ??= new DepartmentListRequestDto();

        var pagedDepartments = await _departmentRepository.GetListingAsync(companyId, request);

        var responseDtos = pagedDepartments.Items
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResult<DepartmentResponseDto>
        {
            Items = responseDtos,
            TotalCount = pagedDepartments.TotalCount,
            PageNumber = pagedDepartments.PageNumber,
            PageSize = pagedDepartments.PageSize
        };
    }

    private static DepartmentResponseDto MapToResponseDto(Department department)
    {
        return new DepartmentResponseDto
        {
            GId = department.GId,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
            IsActive = department.IsActive,
            Created = department.Created,
            LastUpd = department.LastUpd
        };
    }
}
