namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new user in the specified company.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] CreateUserDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var created = await _userService.CreateAsync(authCompanyId, authUserId, request);
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
    /// Retrieves a single user by their public GId.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId(
        [FromRoute] Guid gId,
        [FromQuery] int companyId)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        try
        {
            var user = await _userService.GetByGIdAsync(authCompanyId, gId);

            if (user is null)
            {
                return NotFound(new { message = $"User with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional department, role, search, and active status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetListing(
        [FromQuery] int companyId,
        [FromQuery] UserListRequestDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;

        try
        {
            var result = await _userService.GetListingAsync(authCompanyId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing user's details.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] UpdateUserDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            var updated = await _userService.UpdateAsync(authCompanyId, gId, authUserId, request);

            if (updated is null)
            {
                return NotFound(new { message = $"User with GId '{gId}' was not found for company {authCompanyId}." });
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
    /// Soft deletes a user.
    /// </summary>
    [HttpDelete("{gId:guid}")]
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
            bool isDeleted = await _userService.DeleteAsync(authCompanyId, gId, authUserId);

            if (!isDeleted)
            {
                return NotFound(new { message = $"User with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a user's password following the security password policy.
    /// </summary>
    [HttpPut("{gId:guid}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] Guid gId,
        [FromQuery] int companyId,
        [FromQuery] int userId,
        [FromBody] ChangePasswordDto request)
    {
        // Derive identity from JWT claims, ignoring query parameters
        int authCompanyId = _currentUserService.CompanyId;
        int authUserId = _currentUserService.UserId;

        try
        {
            bool isUpdated = await _userService.ChangePasswordAsync(authCompanyId, gId, authUserId, request);

            if (!isUpdated)
            {
                return NotFound(new { message = $"User with GId '{gId}' was not found for company {authCompanyId}." });
            }

            return Ok(new { message = "Password updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
