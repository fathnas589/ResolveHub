namespace ResolveHub.Services.Notifications;

using System.Net;
using ResolveHub.Core.DTOs.Notifications;

public static class NotificationMessageFactory
{
    public static (string Title, string Message, string ActionUrl) CreateTicketEventCopy(
        string eventType,
        string ticketNumber,
        string subject,
        Guid ticketGId,
        string? actorName = null,
        string? oldStatus = null,
        string? newStatus = null)
    {
        string actionUrl = $"/tickets/{ticketGId}";
        string safeSubject = WebUtility.HtmlEncode(subject);
        string safeTicketNumber = WebUtility.HtmlEncode(ticketNumber);
        string safeActor = WebUtility.HtmlEncode(actorName ?? "A staff member");

        return eventType switch
        {
            NotificationEventTypes.TicketCreated => (
                $"New Ticket: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} \"{safeSubject}\" was created by {safeActor}.",
                actionUrl
            ),
            NotificationEventTypes.TicketAssigned => (
                $"Ticket Assigned: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} \"{safeSubject}\" has been assigned to you.",
                actionUrl
            ),
            NotificationEventTypes.TicketReassigned => (
                $"Ticket Reassigned: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} \"{safeSubject}\" has been reassigned to you by {safeActor}.",
                actionUrl
            ),
            NotificationEventTypes.TicketPublicCommentAdded => (
                $"New Comment on {safeTicketNumber}",
                $"{safeActor} posted a new update on ticket {safeTicketNumber}.",
                actionUrl
            ),
            NotificationEventTypes.TicketStatusChanged => (
                $"Ticket Status Updated: {safeTicketNumber}",
                $"Status of ticket {safeTicketNumber} changed from {oldStatus} to {newStatus}.",
                actionUrl
            ),
            NotificationEventTypes.TicketResolved => (
                $"Ticket Resolved: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} \"{safeSubject}\" has been resolved by {safeActor}.",
                actionUrl
            ),
            NotificationEventTypes.TicketReopened => (
                $"Ticket Reopened: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} \"{safeSubject}\" was reopened by {safeActor}.",
                actionUrl
            ),
            NotificationEventTypes.SlaWarning => (
                $"SLA Warning: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} is approaching its resolution deadline. Action required.",
                actionUrl
            ),
            NotificationEventTypes.FirstResponseSlaBreached => (
                $"First Response SLA Breached: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} has exceeded its First Response target without a response.",
                actionUrl
            ),
            NotificationEventTypes.ResolutionSlaBreached => (
                $"Resolution SLA Breached: {safeTicketNumber}",
                $"Ticket {safeTicketNumber} has exceeded its Resolution target deadline.",
                actionUrl
            ),
            _ => (
                $"Notification for {safeTicketNumber}",
                $"Update received on ticket {safeTicketNumber}.",
                actionUrl
            )
        };
    }

    public static string BuildEmailHtml(
        string title,
        string message,
        string? ticketNumber,
        string? actionUrl,
        string appBaseUrl)
    {
        string fullUrl = $"{appBaseUrl.TrimEnd('/')}{actionUrl}";
        string safeTitle = WebUtility.HtmlEncode(title);
        string safeMessage = WebUtility.HtmlEncode(message);
        string safeTicket = WebUtility.HtmlEncode(ticketNumber ?? "");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; color: #0f172a; }}
        .container {{ max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
        .header {{ background: linear-gradient(135deg, #1e293b, #0f172a); padding: 24px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 20px; font-weight: 700; letter-spacing: -0.02em; }}
        .content {{ padding: 28px 24px; }}
        .title {{ font-size: 18px; font-weight: 700; color: #0f172a; margin: 0 0 12px 0; }}
        .message {{ font-size: 14px; line-height: 1.6; color: #334155; margin: 0 0 24px 0; }}
        .btn {{ display: inline-block; background-color: #2563eb; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; font-size: 14px; text-align: center; }}
        .btn:hover {{ background-color: #1d4ed8; }}
        .footer {{ background-color: #f1f5f9; padding: 16px 24px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>ResolveHub Enterprise</h1>
        </div>
        <div class=""content"">
            <h2 class=""title"">{safeTitle}</h2>
            <p class=""message"">{safeMessage}</p>
            {(string.IsNullOrEmpty(actionUrl) ? "" : $@"<div style=""margin-top: 20px;""><a href=""{fullUrl}"" class=""btn"">View Details in ResolveHub</a></div>")}
        </div>
        <div class=""footer"">
            ResolveHub Enterprise Operations Platform &bull; Automated System Notification
        </div>
    </div>
</body>
</html>";
    }
}
