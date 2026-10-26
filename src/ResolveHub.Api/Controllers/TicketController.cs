namespace ResolveHub.Api.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Tickets;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Creates a new complaint or service request ticket.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto request)
    {
        try
        {
            var result = await _ticketService.CreateTicketAsync(request);
            return CreatedAtAction(nameof(GetByGId), new { gId = result.GId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a ticket by its public GId with role-based access filtering.
    /// </summary>
    [HttpGet("{gId:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGId([FromRoute] Guid gId)
    {
        try
        {
            var result = await _ticketService.GetTicketByGIdAsync(gId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Searches and lists tickets with pagination and role-based filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetListing([FromQuery] TicketListRequestDto request)
    {
        var result = await _ticketService.GetTicketsListingAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Updates controlled details of an existing ticket.
    /// </summary>
    [HttpPut("{gId:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid gId, [FromBody] UpdateTicketDto request)
    {
        try
        {
            var result = await _ticketService.UpdateTicketAsync(gId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Soft-deletes a ticket. ADMIN only.
    /// </summary>
    [HttpDelete("{gId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid gId)
    {
        try
        {
            await _ticketService.DeleteTicketAsync(gId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assigns or reassigns a ticket to an agent or admin.
    /// </summary>
    [HttpPut("{gId:guid}/assign")]
    [Authorize(Roles = "ADMIN,AGENT")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign([FromRoute] Guid gId, [FromBody] AssignTicketDto request)
    {
        try
        {
            var result = await _ticketService.AssignTicketAsync(gId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the status of a ticket adhering to the valid transition state machine.
    /// </summary>
    [HttpPut("{gId:guid}/status")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid gId, [FromBody] UpdateTicketStatusDto request)
    {
        try
        {
            var result = await _ticketService.UpdateTicketStatusAsync(gId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds a comment to a ticket. Internal comments are restricted to AGENT and ADMIN roles.
    /// </summary>
    [HttpPost("{gId:guid}/comments")]
    [ProducesResponseType(typeof(TicketCommentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment([FromRoute] Guid gId, [FromBody] CreateTicketCommentDto request)
    {
        try
        {
            var result = await _ticketService.AddCommentAsync(gId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all comments for a ticket. Internal comments are excluded for USER role.
    /// </summary>
    [HttpGet("{gId:guid}/comments")]
    [ProducesResponseType(typeof(IEnumerable<TicketCommentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments([FromRoute] Guid gId)
    {
        try
        {
            var result = await _ticketService.GetCommentsAsync(gId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves full audit history for a ticket.
    /// </summary>
    [HttpGet("{gId:guid}/history")]
    [ProducesResponseType(typeof(IEnumerable<TicketHistoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory([FromRoute] Guid gId)
    {
        try
        {
            var result = await _ticketService.GetHistoryAsync(gId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves the SLA tracking snapshot and live health status for a ticket.
    /// </summary>
    [HttpGet("{gId:guid}/sla")]
    [ProducesResponseType(typeof(ResolveHub.Core.DTOs.Sla.TicketSlaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketSla([FromRoute] Guid gId)
    {
        try
        {
            var result = await _ticketService.GetTicketSlaAsync(gId);
            if (result is null)
            {
                return NotFound(new { message = $"No SLA tracking record found for ticket '{gId}'." });
            }
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
