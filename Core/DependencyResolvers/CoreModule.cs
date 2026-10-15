using Core.CrossCuttingConcerns.Caching;
using Core.CrossCuttingConcerns.Caching.Microsoft;
using Core.Utilities.IoC;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.DependencyResolvers;

public class CoreModule : ICoreModule
{
    public void Load(IServiceCollection serviceColleciton)
    {
        serviceColleciton.AddMemoryCache();
        serviceColleciton.AddHttpContextAccessor();
        serviceColleciton.AddSingleton<ICacheManager,MemoryCacheManager>();
        serviceColleciton.AddHttpClient("gateway", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var gatewayUrl = config.GetValue<string>("GatewayApiBaseUrl");
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                client.BaseAddress = new Uri(gatewayUrl);
            }
        });
        serviceColleciton.AddHttpClient("KIP", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var kipUrl = config.GetValue<string>("KIPApiBaseUrl");
            if (!string.IsNullOrEmpty(kipUrl))
            {
                client.BaseAddress = new Uri(kipUrl);
            }
        });

    }
}