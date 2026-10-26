namespace ResolveHub.DataAccess.Connection;

using System;
using System.Threading.Tasks;

public class DatabaseHealthResult
{
    public bool IsConnected { get; set; }
    public string? DatabaseName { get; set; }
    public DateTime? ServerTime { get; set; }
    public string? ServerVersion { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IDatabaseHealthCheck
{
    Task<DatabaseHealthResult> GetDatabaseHealthAsync();
}
