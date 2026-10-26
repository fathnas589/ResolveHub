namespace ResolveHub.Api.Controllers;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Auth;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and issues a JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registers a new self-service requester (Role: USER) and returns a JWT token.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
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
    /// Retrieves the current authenticated user's claims and profile summary.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        Guid userGId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(subClaim))
        {
            Guid.TryParse(subClaim, out userGId);
        }

        int companyId = 0;
        var companyClaim = User.FindFirstValue("companyId");
        if (!string.IsNullOrWhiteSpace(companyClaim))
        {
            int.TryParse(companyClaim, out companyId);
        }

        var userName = User.FindFirstValue("userName") ?? User.Identity?.Name ?? string.Empty;
        var fullName = User.FindFirstValue("fullName") ?? string.Empty;
        var email = User.FindFirstValue("email");
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? string.Empty;

        Guid? departmentGId = null;
        var deptGIdClaim = User.FindFirstValue("departmentGId");
        if (!string.IsNullOrWhiteSpace(deptGIdClaim) && Guid.TryParse(deptGIdClaim, out var parsedDeptGId))
        {
            departmentGId = parsedDeptGId;
        }

        var departmentName = User.FindFirstValue("departmentName");

        var currentUser = new CurrentUserDto
        {
            UserGId = userGId,
            UserName = userName,
            FullName = fullName,
            Email = email,
            CompanyId = companyId,
            Role = role,
            DepartmentGId = departmentGId,
            DepartmentName = departmentName
        };

        return Ok(currentUser);
    }
}
