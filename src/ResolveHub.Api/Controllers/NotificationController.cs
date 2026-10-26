namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Services.Interfaces;
using ResolveHub.Services.Notifications;

[ApiController]
[Route("api/[controller]")]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationController(
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves paginated notifications for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetListing([FromQuery] NotificationListRequestDto request)
    {
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        var result = await _notificationService.GetListingAsync(companyId, userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Gets the count of unread notifications for the authenticated user.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadNotificationCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount()
    {
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        int count = await _notificationService.GetUnreadCountAsync(companyId, userId);
        return Ok(new UnreadNotificationCountDto { UnreadCount = count });
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPut("{gId:guid}/read")]
    [HttpPost("{gId:guid}/read")]
    [HttpPost("{gId:guid}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkRead([FromRoute] Guid gId)
    {
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        bool updated = await _notificationService.MarkReadAsync(companyId, userId, gId);
        if (!updated)
        {
            return NotFound(new { message = "Notification not found or already processed." });
        }

        return Ok(new { message = "Notification marked as read." });
    }

    /// <summary>
    /// Marks all unread notifications as read for the authenticated user.
    /// </summary>
    [HttpPut("read-all")]
    [HttpPost("read-all")]
    [HttpPost("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead()
    {
        int companyId = _currentUserService.CompanyId;
        int userId = _currentUserService.UserId;

        int count = await _notificationService.MarkAllReadAsync(companyId, userId);
        return Ok(new { message = $"Marked {count} notification(s) as read.", count });
    }
}
