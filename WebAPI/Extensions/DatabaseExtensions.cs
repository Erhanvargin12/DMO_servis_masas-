using Core.DataAccess.EntityFramework;
using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebAPI.Extensions;

public static class DatabaseExtensions
{
    public static void AddCustomDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();

        var connection = configuration.GetConnectionString("MsSqlContext");

        services.AddDbContext<MsSqlDbContext>((sp, options) => {
            options.UseSqlServer(connection);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });
    }
}
