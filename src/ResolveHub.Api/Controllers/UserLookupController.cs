namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Services.Interfaces;

/// <summary>
/// Read-only user lookups needed by agents (e.g. the ticket assignment dropdown).
/// Kept separate from <see cref="UserController"/>, which is restricted to ADMIN.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize(Roles = "ADMIN,AGENT")]
public class UserLookupController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserLookupController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Lists active agents that tickets can be assigned to, optionally filtered by department.
    /// </summary>
    [HttpGet("assignable")]
    [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssignable([FromQuery] Guid? departmentGId)
    {
        var request = new UserListRequestDto
        {
            DepartmentGId = departmentGId,
            RoleCode = "AGENT",
            IsActive = true,
            PageNumber = 1,
            PageSize = 100
        };

        try
        {
            var result = await _userService.GetListingAsync(_currentUserService.CompanyId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
