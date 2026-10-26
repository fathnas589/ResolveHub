namespace ResolveHub.Services.Notifications;

using System.Threading;
using System.Threading.Tasks;

public interface INotificationDeliveryService
{
    Task<int> ProcessPendingDeliveriesAsync(CancellationToken cancellationToken = default);
}
