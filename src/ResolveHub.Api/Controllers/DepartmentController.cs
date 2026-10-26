namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Departments;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentController(IDepartmentService departmentService, ICurrentUserService currentUserService)
    {
        _departmentService = departmentService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new department for the specified company.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] CreateDepartmentDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var created = await _departmentService.CreateAsync(authCompanyId, authUserId, request);
            return CreatedAtAction(nameof(GetByGId), new { gId = created.GId, companyId = authCompanyId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a single department by its public GId.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId(
        [FromRoute] Guid gId,
        [FromQuery] int companyId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        var department = await _departmentService.GetByGIdAsync(authCompanyId, gId);

        if (department is null)
        {
            return NotFound(new { message = $"Department with GId '{gId}' was not found for company {authCompanyId}." });
        }

        return Ok(department);
    }

    /// <summary>
    /// Retrieves a paginated list of departments with optional search and active status filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListing(
        [FromQuery] int companyId,
        [FromQuery] DepartmentListRequestDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        var result = await _departmentService.GetListingAsync(authCompanyId, request);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] UpdateDepartmentDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var updated = await _departmentService.UpdateAsync(authCompanyId, gId, authUserId, request);

            if (updated is null)
            {
                return NotFound(new { message = $"Department with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Soft deletes a department.
    /// </summary>
    [HttpDelete("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        bool isDeleted = await _departmentService.DeleteAsync(authCompanyId, gId, authUserId);

        if (!isDeleted)
        {
            return NotFound(new { message = $"Department with GId '{gId}' was not found for company {authCompanyId}." });
        }

        return NoContent();
    }
}
