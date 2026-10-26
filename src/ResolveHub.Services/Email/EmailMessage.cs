namespace ResolveHub.Services.Email;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }

    public static EmailSendResult Ok(string? messageId = null) => new() { Success = true, ProviderMessageId = messageId };
    public static EmailSendResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}

public class EmailOptions
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = false;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "notifications@resolvehub.local";
    public string FromName { get; set; } = "ResolveHub Notifications";
    public string AppBaseUrl { get; set; } = "http://localhost:4200";
}
