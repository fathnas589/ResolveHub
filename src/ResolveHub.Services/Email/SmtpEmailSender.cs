namespace ResolveHub.Services.Email;

using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.Enabled)
        {
            _logger.LogInformation("Email delivery is disabled in configuration. Skipping SMTP send to {To} with Subject '{Subject}'.",
                message.To, message.Subject);
            return EmailSendResult.Ok("disabled-mock-delivery");
        }

        if (string.IsNullOrWhiteSpace(message.To))
        {
            return EmailSendResult.Fail("Recipient email address is missing or empty.");
        }

        try
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(message.To, message.RecipientName));

            using var smtpClient = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Successfully dispatched email to {To} with Subject '{Subject}'.", message.To, message.Subject);
            return EmailSendResult.Ok(Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver email to {To} via SMTP {Host}:{Port}.", message.To, _options.Host, _options.Port);
            return EmailSendResult.Fail(ex.Message);
        }
    }
}
