namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.SubCategories;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubCategoryController : ControllerBase
{
    private readonly ISubCategoryService _subCategoryService;
    private readonly ICurrentUserService _currentUserService;

    public SubCategoryController(ISubCategoryService subCategoryService, ICurrentUserService currentUserService)
    {
        _subCategoryService = subCategoryService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new subcategory under a category for the specified company.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(SubCategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] CreateSubCategoryDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var created = await _subCategoryService.CreateAsync(authCompanyId, authUserId, request);
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
    /// Retrieves a single subcategory by its public GId.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [ProducesResponseType(typeof(SubCategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId(
        [FromRoute] Guid gId,
        [FromQuery] int companyId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        try
        {
            var subCategory = await _subCategoryService.GetByGIdAsync(authCompanyId, gId);

            if (subCategory is null)
            {
                return NotFound(new { message = $"SubCategory with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return Ok(subCategory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of subcategories with optional category, department, search, and active status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SubCategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetListing(
        [FromQuery] int companyId,
        [FromQuery] SubCategoryListRequestDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        try
        {
            var result = await _subCategoryService.GetListingAsync(authCompanyId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing subcategory.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(SubCategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] UpdateSubCategoryDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var updated = await _subCategoryService.UpdateAsync(authCompanyId, gId, authUserId, request);

            if (updated is null)
            {
                return NotFound(new { message = $"SubCategory with GId '{gId}' was not found for company {authCompanyId}." });
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
    /// Soft deletes a subcategory.
    /// </summary>
    [HttpDelete("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            bool isDeleted = await _subCategoryService.DeleteAsync(authCompanyId, gId, authUserId);

            if (!isDeleted)
            {
                return NotFound(new { message = $"SubCategory with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
