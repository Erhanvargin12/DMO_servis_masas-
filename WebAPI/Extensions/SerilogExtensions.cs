using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace WebAPI.Extensions;

public static class SerilogExtensions
{
    public static void AddCustomSerilog(this WebApplicationBuilder builder)
    {
        var mssqlConn = builder.Configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration:ConnectionString").Value;
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

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File($"{logFolder}/log-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.MSSqlServer(
                connectionString: mssqlConn ?? "Server=(localdb)\\mssqllocaldb;Database=CoreDev;Trusted_Connection=True;TrustServerCertificate=True;",
                sinkOptions: new MSSqlServerSinkOptions { TableName = "HttpLogs", AutoCreateSqlTable = true },
                columnOptions: columnOptions
            )
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
