namespace ResolveHub.DataAccess.Connection;

using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

public class DatabaseHealthCheck : IDatabaseHealthCheck
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DatabaseHealthCheck(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DatabaseHealthResult> GetDatabaseHealthAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT DB_NAME() AS DatabaseName, GETDATE() AS ServerTime, @@VERSION AS ServerVersion;";
            var result = await connection.QuerySingleAsync<DatabaseHealthResult>(sql);
            result.IsConnected = true;
            return result;
        }
        catch (Exception ex)
        {
            return new DatabaseHealthResult
            {
                IsConnected = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
