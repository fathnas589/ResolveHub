namespace ResolveHub.Services.Workers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResolveHub.Services.Notifications;

public class NotificationDeliveryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationDeliveryWorker> _logger;

    public NotificationDeliveryWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<NotificationDeliveryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool enabled = _configuration.GetValue<bool>("NotificationDelivery:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("Notification Delivery Worker is disabled in configuration.");
            return;
        }

        // With email disabled the sender would report every delivery as sent (mock), permanently
        // marking queued emails DELIVERED. Leave them PENDING so they go out once SMTP is enabled.
        if (!_configuration.GetValue<bool>("Email:Enabled", false))
        {
            _logger.LogInformation("Notification Delivery Worker is idle because Email:Enabled is false; queued deliveries remain PENDING.");
            return;
        }

        int intervalSeconds = _configuration.GetValue<int>("NotificationDelivery:PollIntervalSeconds", 15);
        if (intervalSeconds <= 0)
        {
            intervalSeconds = 15;
        }

        _logger.LogInformation("Notification Delivery Worker started with poll interval of {IntervalSeconds} seconds.", intervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var deliveryService = scope.ServiceProvider.GetRequiredService<INotificationDeliveryService>();

                    int processed = await deliveryService.ProcessPendingDeliveriesAsync(stoppingToken);
                    if (processed > 0)
                    {
                        _logger.LogInformation("Notification Delivery Worker dispatched {Count} item(s).", processed);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred during notification delivery cycle.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Notification Delivery Worker execution was cancelled.");
        }
    }
}
