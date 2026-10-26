namespace ResolveHub.Services.Workers;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResolveHub.Core.Interfaces.Repositories;

public class SlaMonitoringWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SlaMonitoringWorker> _logger;

    public SlaMonitoringWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SlaMonitoringWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalSeconds = _configuration.GetValue<int>("SlaMonitoring:IntervalSeconds", 60);
        if (intervalSeconds <= 0)
        {
            intervalSeconds = 60;
        }

        _logger.LogInformation("SLA Monitoring Worker started with interval of {IntervalSeconds} seconds.", intervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var ticketSlaRepository = scope.ServiceProvider.GetRequiredService<ITicketSlaRepository>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<Notifications.INotificationService>();

                    var nowUtc = DateTime.UtcNow;

                    // 1. Process SLA escalation warnings
                    var warningEvents = (await ticketSlaRepository.CheckWarningsAsync(nowUtc)).ToList();
                    if (warningEvents.Count > 0)
                    {
                        _logger.LogWarning("SLA Monitoring Worker detected {Count} SLA escalation warning(s) at {TimeUtc}.", warningEvents.Count, nowUtc);
                        foreach (var warning in warningEvents)
                        {
                            try
                            {
                                await notificationService.PublishSlaWarningAsync(
                                    warning.CompanyId,
                                    warning.TicketId,
                                    warning.TicketGId,
                                    warning.TicketNumber,
                                    warning.Subject,
                                    warning.AssignedToUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to publish SLA warning notification for Ticket {TicketNumber}", warning.TicketNumber);
                            }
                        }
                    }

                    // 2. Process SLA breaches
                    var breachedEvents = (await ticketSlaRepository.CheckBreachesAsync(nowUtc)).ToList();
                    if (breachedEvents.Count > 0)
                    {
                        _logger.LogWarning("SLA Monitoring Worker detected {Count} new SLA breach(es) at {TimeUtc}.", breachedEvents.Count, nowUtc);
                        foreach (var breach in breachedEvents)
                        {
                            _logger.LogWarning("Ticket {TicketNumber} (Company {CompanyId}) breached {BreachType} SLA. Deadline was {DueAt:u}.",
                                breach.TicketNumber, breach.CompanyId, breach.BreachType, breach.DueAt);

                            try
                            {
                                await notificationService.PublishSlaBreachAsync(
                                    breach.CompanyId,
                                    breach.TicketId,
                                    breach.TicketGId,
                                    breach.TicketNumber,
                                    breach.Subject,
                                    breach.BreachType,
                                    breach.AssignedToUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to publish SLA breach notification for Ticket {TicketNumber}", breach.TicketNumber);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during SLA breach detection cycle.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SLA Monitoring Worker execution was cancelled.");
        }
    }
}
