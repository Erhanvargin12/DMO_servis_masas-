using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace WebAPI.Extensions;

public static class SerilogExtensions
{
    public static void AddCustomSerilog(this WebApplicationBuilder builder)
    {
        var mssqlConn = builder.Configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration:ConnectionString").Value;
        // Keep connection string as-is from configuration. Legacy configs used "DevCore";
        // continue to support that by normalizing value but avoid crashing the app if DB is unreachable.
        if (mssqlConn != null && mssqlConn.Contains("DevCore")) mssqlConn = mssqlConn.Replace("DevCore", "CoreDev");

        var logFolder = builder.Configuration.GetSection("SeriLogConfigurations:FileLogConfiguration:FolderPath").Value ?? "logs";

        var columnOptions = new ColumnOptions();
        columnOptions.AdditionalColumns = new List<SqlColumn>
        {
            new SqlColumn { ColumnName = "CreatedUserId", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "CreatedSurrogateId", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "Application", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "HttpMethod", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "StatusCode", DataType = System.Data.SqlDbType.Int },
            new SqlColumn { ColumnName = "Path", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "RequestBody", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "ResponseBody", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "Elapsed", DataType = System.Data.SqlDbType.Decimal },
            new SqlColumn { ColumnName = "RemoteIpAddress", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "SourceContext", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 },
            new SqlColumn { ColumnName = "TraceIdentifier", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1 }
        };

        var defaultConn = mssqlConn ?? "Server=(localdb)\\mssqllocaldb;Database=CoreDev;Trusted_Connection=True;TrustServerCertificate=True;";

        var baseLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File($"{logFolder}/log-.txt", rollingInterval: RollingInterval.Day);

        try
        {
            // Try to enable MSSqlServer sink; if the DB is unavailable or credentials are invalid
            // the CreateLogger() call may throw. In that case fall back to the file+console logger.
            var withSql = baseLogger.WriteTo.MSSqlServer(
                connectionString: defaultConn,
                sinkOptions: new MSSqlServerSinkOptions { TableName = "HttpLogs", AutoCreateSqlTable = true },
                columnOptions: columnOptions
            );

            Log.Logger = withSql.CreateLogger();
        }
        catch (Exception ex)
        {
            // If SQL sink cannot be initialized, fall back to file+console sink so the app still starts.
            // Write the failure reason to stderr so developers can see it during startup.
            try { Console.Error.WriteLine($"[Serilog] SQL sink initialization failed: {ex.Message}"); } catch { }
            baseLogger.WriteTo.Console();
            Log.Logger = baseLogger.CreateLogger();
        }

        builder.Host.UseSerilog();
    }
}
