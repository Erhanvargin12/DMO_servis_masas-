using Autofac.Core;
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
        serviceColleciton.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();
        serviceColleciton.AddSingleton<ICacheManager,MemoryCacheManager>();
        serviceColleciton.AddHttpClient("gateway", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config.GetValue<string>("GatewayApiBaseUrl")!);
        });

    }
}