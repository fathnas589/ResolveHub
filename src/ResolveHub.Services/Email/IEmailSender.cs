namespace ResolveHub.Services.Email;

using System.Threading;
using System.Threading.Tasks;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
