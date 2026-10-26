namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SlaPolicyController : ControllerBase
{
    private readonly ISlaPolicyService _slaPolicyService;
    private readonly ICurrentUserService _currentUserService;

    public SlaPolicyController(
        ISlaPolicyService slaPolicyService,
        ICurrentUserService currentUserService)
    {
        _slaPolicyService = slaPolicyService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new SLA policy for the tenant. Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(SlaPolicyResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSlaPolicyDto request)
    {
        try
        {
            int companyId = _currentUserService.CompanyId;
            int userId = _currentUserService.UserId;

            var created = await _slaPolicyService.CreateAsync(companyId, userId, request);
            return CreatedAtAction(nameof(GetByGId), new { gId = created.GId }, created);
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
    /// Updates an existing SLA policy by its GId. Admin only.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(SlaPolicyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid gId, [FromBody] UpdateSlaPolicyDto request)
    {
        try
        {
            int companyId = _currentUserService.CompanyId;
            int userId = _currentUserService.UserId;

            var updated = await _slaPolicyService.UpdateAsync(companyId, gId, userId, request);
            if (updated is null)
            {
                return NotFound(new { message = $"SLA policy with ID '{gId}' was not found." });
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
    /// Soft deletes an SLA policy. Admin only.
    /// </summary>
    [HttpDelete("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid gId)
    {
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        bool deleted = await _slaPolicyService.DeleteAsync(companyId, gId, userId);
        if (!deleted)
        {
            return NotFound(new { message = $"SLA policy with ID '{gId}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Retrieves a single SLA policy by its GId.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [Authorize(Roles = "ADMIN,AGENT")]
    [ProducesResponseType(typeof(SlaPolicyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId([FromRoute] Guid gId)
    {
        int companyId = _currentUserService.CompanyId;

        var policy = await _slaPolicyService.GetByGIdAsync(companyId, gId);
        if (policy is null)
        {
            return NotFound(new { message = $"SLA policy with ID '{gId}' was not found." });
        }

        return Ok(policy);
    }

    /// <summary>
    /// Retrieves paginated SLA policies for the authenticated company.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,AGENT")]
    [ProducesResponseType(typeof(PagedResult<SlaPolicyResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListing([FromQuery] SlaPolicyListRequestDto request)
    {
        int companyId = _currentUserService.CompanyId;

        var result = await _slaPolicyService.GetListingAsync(companyId, request);
        return Ok(result);
    }
}
