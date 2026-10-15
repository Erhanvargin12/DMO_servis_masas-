using Entities.Concrete;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using WebUI.Core.Services.Abstract;

namespace WebUI.Core.Middleware
{
    public class MenuAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MenuAccessMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private const string CacheKey = "InactiveMenuUrls";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public MenuAccessMiddleware(RequestDelegate next, ILogger<MenuAccessMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
        {
            await _next(context);
        }

        private static bool ShouldSkip(string? path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            var skipPaths = new[]
            {
                "/account/",
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/favicon",
                "/.well-known"
            };

            return skipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class MenuAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseMenuAccess(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MenuAccessMiddleware>();
        }
    }
}

