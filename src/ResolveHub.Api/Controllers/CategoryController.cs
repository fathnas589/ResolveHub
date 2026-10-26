namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Categories;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUserService;

    public CategoryController(ICategoryService categoryService, ICurrentUserService currentUserService)
    {
        _categoryService = categoryService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new category under a department for the specified company.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] CreateCategoryDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var created = await _categoryService.CreateAsync(authCompanyId, authUserId, request);
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
    /// Retrieves a single category by its public GId.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId(
        [FromRoute] Guid gId,
        [FromQuery] int companyId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        var category = await _categoryService.GetByGIdAsync(authCompanyId, gId);

        if (category is null)
        {
            return NotFound(new { message = $"Category with GId '{gId}' was not found for company {authCompanyId}." });
        }

        return Ok(category);
    }

    /// <summary>
    /// Retrieves a paginated list of categories with optional department, search, and active status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListing(
        [FromQuery] int companyId,
        [FromQuery] CategoryListRequestDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        var result = await _categoryService.GetListingAsync(authCompanyId, request);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] UpdateCategoryDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var updated = await _categoryService.UpdateAsync(authCompanyId, gId, authUserId, request);

            if (updated is null)
            {
                return NotFound(new { message = $"Category with GId '{gId}' was not found for company {authCompanyId}." });
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
    /// Soft deletes a category.
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

        bool isDeleted = await _categoryService.DeleteAsync(authCompanyId, gId, authUserId);

        if (!isDeleted)
        {
            return NotFound(new { message = $"Category with GId '{gId}' was not found for company {authCompanyId}." });
        }

        return NoContent();
    }
}
