namespace ResolveHub.DataAccess.Connection;

using System.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
