namespace ResolveHub.Services.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Email;

public class NotificationDeliveryService : INotificationDeliveryService
{
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly IEmailSender _emailSender;
    private readonly NotificationDeliveryOptions _deliveryOptions;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<NotificationDeliveryService> _logger;

    public NotificationDeliveryService(
        INotificationDeliveryRepository deliveryRepository,
        IEmailSender emailSender,
        IOptions<NotificationDeliveryOptions> deliveryOptions,
        IOptions<EmailOptions> emailOptions,
        ILogger<NotificationDeliveryService> logger)
    {
        _deliveryRepository = deliveryRepository;
        _emailSender = emailSender;
        _deliveryOptions = deliveryOptions.Value;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task<int> ProcessPendingDeliveriesAsync(CancellationToken cancellationToken = default)
    {
        var claimedDeliveries = await _deliveryRepository.ClaimPendingDeliveriesAsync(_deliveryOptions.BatchSize, DateTime.UtcNow);
        if (claimedDeliveries.Count == 0)
        {
            return 0;
        }

        _logger.LogInformation("Claimed {Count} notification delivery item(s) for dispatch.", claimedDeliveries.Count);

        int processedCount = 0;

        foreach (var delivery in claimedDeliveries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                string htmlBody = NotificationMessageFactory.BuildEmailHtml(
                    delivery.Title,
                    delivery.Message,
                    delivery.TicketNumber,
                    delivery.ActionUrl,
                    _emailOptions.AppBaseUrl);

                var emailMessage = new EmailMessage
                {
                    To = delivery.RecipientEmail,
                    RecipientName = delivery.RecipientName,
                    Subject = delivery.Title,
                    HtmlBody = htmlBody
                };

                var sendResult = await _emailSender.SendAsync(emailMessage, cancellationToken);

                if (sendResult.Success)
                {
                    await _deliveryRepository.UpdateDeliveryStatusAsync(delivery.DeliveryId, isSuccess: true);
                    _logger.LogInformation("Notification delivery {DeliveryId} to {RecipientEmail} marked DELIVERED.",
                        delivery.DeliveryId, delivery.RecipientEmail);
                }
                else
                {
                    DateTime nextAttempt = CalculateNextAttempt(delivery.AttemptCount);
                    await _deliveryRepository.UpdateDeliveryStatusAsync(
                        delivery.DeliveryId,
                        isSuccess: false,
                        lastError: sendResult.ErrorMessage,
                        nextAttemptAt: nextAttempt,
                        maxAttempts: _deliveryOptions.MaxAttempts);

                    _logger.LogWarning("Notification delivery {DeliveryId} to {RecipientEmail} failed (Attempt {Attempt}): {Error}",
                        delivery.DeliveryId, delivery.RecipientEmail, delivery.AttemptCount, sendResult.ErrorMessage);
                }

                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing delivery {DeliveryId} to {RecipientEmail}.",
                    delivery.DeliveryId, delivery.RecipientEmail);

                DateTime nextAttempt = CalculateNextAttempt(delivery.AttemptCount);
                await _deliveryRepository.UpdateDeliveryStatusAsync(
                    delivery.DeliveryId,
                    isSuccess: false,
                    lastError: ex.Message,
                    nextAttemptAt: nextAttempt,
                    maxAttempts: _deliveryOptions.MaxAttempts);
            }
        }

        return processedCount;
    }

    public static DateTime CalculateNextAttempt(int attemptCount, DateTime? nowUtc = null)
    {
        DateTime baseTime = nowUtc ?? DateTime.UtcNow;

        int delayMinutes = attemptCount switch
        {
            <= 1 => 1,
            2 => 5,
            3 => 15,
            _ => 60
        };

        return baseTime.AddMinutes(delayMinutes);
    }
}
