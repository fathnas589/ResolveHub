using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ResolveHub.DbInitializer;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("        ResolveHub - Database Bootstrapper        ");
        Console.WriteLine("==================================================\n");

        try
        {
            // 1. Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string? targetConnectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(targetConnectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] 'DefaultConnection' connection string was not found in configuration.");
                Console.ResetColor();
                return 1;
            }

            var connectionBuilder = new SqlConnectionStringBuilder(targetConnectionString);
            string targetDatabaseName = string.IsNullOrWhiteSpace(connectionBuilder.InitialCatalog)
                ? "ResolveHubDb"
                : connectionBuilder.InitialCatalog;

            Console.WriteLine($"Target Server   : {connectionBuilder.DataSource}");
            Console.WriteLine($"Target Database : {targetDatabaseName}\n");

            // 2. Ensure database exists via 'master'
            connectionBuilder.InitialCatalog = "master";
            string masterConnectionString = connectionBuilder.ConnectionString;

            Console.WriteLine("[PHASE 1] Connecting to SQL Server 'master' database...");
            await using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                await masterConnection.OpenAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SUCCESS] Connected to SQL Server master database.");
                Console.ResetColor();

                const string checkDbSql = "SELECT COUNT(1) FROM sys.databases WHERE name = @DbName;";
                int dbCount = await masterConnection.ExecuteScalarAsync<int>(checkDbSql, new { DbName = targetDatabaseName });

                if (dbCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[INFO] Database '{targetDatabaseName}' already exists.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"[INFO] Creating database '{targetDatabaseName}'...");
                    string sanitizedDbName = targetDatabaseName.Replace("]", "]]");
                    await masterConnection.ExecuteAsync($"CREATE DATABASE [{sanitizedDbName}];");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[SUCCESS] Database '{targetDatabaseName}' created.");
                    Console.ResetColor();
                }
            }

            // 3. Connect to the target database and execute scripts
            Console.WriteLine($"\n[PHASE 2] Connecting to '{targetDatabaseName}' to execute migration scripts...");
            await using (var targetConnection = new SqlConnection(targetConnectionString))
            {
                await targetConnection.OpenAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Connected to '{targetDatabaseName}'.");
                Console.ResetColor();

                string scriptsBasePath = Path.Combine(AppContext.BaseDirectory, "Scripts");

                if (!Directory.Exists(scriptsBasePath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] Scripts directory not found at '{scriptsBasePath}'.");
                    Console.ResetColor();
                    return 0;
                }

                // Get all .sql files ordered deterministically by directory and file name
                var scriptFiles = Directory.GetFiles(scriptsBasePath, "*.sql", SearchOption.AllDirectories)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                Console.WriteLine($"\n[INFO] Found {scriptFiles.Count} SQL script(s) to execute:\n");

                foreach (var scriptPath in scriptFiles)
                {
                    string relativePath = Path.GetRelativePath(scriptsBasePath, scriptPath);
                    Console.Write($" -> Executing: {relativePath} ... ");

                    string sqlContent = await File.ReadAllTextAsync(scriptPath);

                    if (string.IsNullOrWhiteSpace(sqlContent))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("SKIPPED (Empty)");
                        Console.ResetColor();
                        continue;
                    }

                    // Execute script
                    await targetConnection.ExecuteAsync(sqlContent);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();
                }

                // 4. Verify Database Objects
                Console.WriteLine("\n[PHASE 3] Verifying database schema and stored procedures...");

                // Verify Sequence
                int seqExists = await targetConnection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM sys.sequences WHERE name = 'Seq_TicketNumber' AND schema_id = SCHEMA_ID('dbo');");
                if (seqExists > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" [✔] Sequence 'dbo.Seq_TicketNumber' verified.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" [✖] Sequence 'dbo.Seq_TicketNumber' NOT found!");
                    Console.ResetColor();
                }

                // Verify Tables
                var expectedTables = new[] { "Department", "Category", "SubCategory", "UserMaster", "Ticket", "TicketHistory", "TicketComment" };
                foreach (var tableName in expectedTables)
                {
                    int tableExists = await targetConnection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName AND schema_id = SCHEMA_ID('dbo');",
                        new { TableName = tableName });

                    if (tableExists > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" [✔] Table 'dbo.{tableName}' verified.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" [✖] Table 'dbo.{tableName}' NOT found!");
                        Console.ResetColor();
                    }
                }

                // Verify Foreign Keys
                var expectedFks = new[]
                {
                    ("FK_Category_Department", "dbo.Category"),
                    ("FK_SubCategory_Category", "dbo.SubCategory"),
                    ("FK_UserMaster_Department", "dbo.UserMaster"),
                    ("FK_Ticket_CreatedByUser", "dbo.Ticket"),
                    ("FK_Ticket_AssignedToUser", "dbo.Ticket"),
                    ("FK_Ticket_Department", "dbo.Ticket"),
                    ("FK_Ticket_Category", "dbo.Ticket"),
                    ("FK_Ticket_SubCategory", "dbo.Ticket"),
                    ("FK_TicketHistory_Ticket", "dbo.TicketHistory"),
                    ("FK_TicketHistory_CreatedByUser", "dbo.TicketHistory"),
                    ("FK_TicketComment_Ticket", "dbo.TicketComment"),
                    ("FK_TicketComment_CreatedByUser", "dbo.TicketComment")
                };

                foreach (var (fkName, parentTable) in expectedFks)
                {
                    int fkExists = await targetConnection.ExecuteScalarAsync<int>(
                        $"SELECT COUNT(1) FROM sys.foreign_keys WHERE name = '{fkName}' AND parent_object_id = OBJECT_ID('{parentTable}');");
                    if (fkExists > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" [✔] Foreign Key '{fkName}' verified.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" [✖] Foreign Key '{fkName}' NOT found!");
                        Console.ResetColor();
                    }
                }

                // Verify Stored Procedures
                var expectedProcedures = new[]
                {
                    "Department_Insert",
                    "Department_Update",
                    "Department_Delete",
                    "Department_Get",
                    "Department_Listing",
                    "Department_DuplicateCheck",
                    "Category_Insert",
                    "Category_Update",
                    "Category_Delete",
                    "Category_Get",
                    "Category_Listing",
                    "Category_DuplicateCheck",
                    "SubCategory_Insert",
                    "SubCategory_Update",
                    "SubCategory_Delete",
                    "SubCategory_Get",
                    "SubCategory_Listing",
                    "SubCategory_DuplicateCheck",
                    "UserMaster_Insert",
                    "UserMaster_Update",
                    "UserMaster_Delete",
                    "UserMaster_Get",
                    "UserMaster_Listing",
                    "UserMaster_DuplicateCheck",
                    "UserMaster_GetForLogin",
                    "UserMaster_UpdatePassword",
                    "Ticket_Insert",
                    "Ticket_Get",
                    "Ticket_Listing",
                    "Ticket_Update",
                    "Ticket_Assign",
                    "Ticket_UpdateStatus",
                    "Ticket_Delete",
                    "TicketComment_Insert",
                    "TicketComment_Listing",
                    "TicketHistory_Listing"
                };

                const string checkSpSql = "SELECT name FROM sys.procedures WHERE schema_id = SCHEMA_ID('dbo');";
                var actualProcedures = (await targetConnection.QueryAsync<string>(checkSpSql)).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var sp in expectedProcedures)
                {
                    if (actualProcedures.Contains(sp))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" [✔] Stored Procedure 'dbo.{sp}' verified.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" [✖] Stored Procedure 'dbo.{sp}' NOT found!");
                        Console.ResetColor();
                    }
                }

                // 5. Bootstrap development admin user
                Console.WriteLine("\n[PHASE 4] Checking/bootstrapping development admin user...");
                int adminExists = await targetConnection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM dbo.UserMaster WHERE CompanyId = 1 AND UserName = 'admin' AND IsDeleted = 0;");
                if (adminExists == 0)
                {
                    Console.WriteLine(" [INFO] Bootstrapping development admin user 'admin' for CompanyId = 1...");
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
                    string passwordHash = hasher.HashPassword("admin", "Admin@123");
                    await targetConnection.ExecuteAsync(@"
                        INSERT INTO dbo.UserMaster
                        (GId, CompanyId, DepartmentId, UserName, FullName, Email, Phone, PasswordHash, RoleCode, IsActive, IsDeleted, Created, CreatedBy)
                        VALUES
                        (NEWID(), 1, NULL, 'admin', 'ResolveHub Admin', 'admin@resolvehub.local', '1234567890', @PasswordHash, 'ADMIN', 1, 0, SYSUTCDATETIME(), 1);",
                        new { PasswordHash = passwordHash });
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" [✔] Development admin user created (Username: admin, Role: ADMIN).");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" [✔] Development admin user already exists.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n==================================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Database initialization completed successfully! ");
            Console.ResetColor();
            Console.WriteLine("==================================================");

            return 0;
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[SQL ERROR #{ex.Number}] {ex.Message}");
            Console.WriteLine("\nTroubleshooting tips:");
            Console.WriteLine("1. Ensure SQL Server service is running.");
            Console.WriteLine("2. Verify SQL script syntax and permissions.");
            Console.ResetColor();
            return 1;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}
