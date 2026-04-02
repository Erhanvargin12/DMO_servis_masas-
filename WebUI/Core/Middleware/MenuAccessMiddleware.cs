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
            var path = context.Request.Path.Value?.ToLower();

            if (ShouldSkip(path))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var isAdmin = context.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                await _next(context);
                return;
            }

            if (!cache.TryGetValue(CacheKey, out List<string>? inactiveUrls) || inactiveUrls == null)
            {
                using var scope = _scopeFactory.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();

                var token = context.User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                    apiClient.SetBearerToken(token);

                var menus = await apiClient.GetAsync<List<Menu>>("menu/getall");
                inactiveUrls = menus.Data?.Where(u => u.IsActive == false).Select(u => u.Url).ToList() ?? new List<string>();
                cache.Set(CacheKey, inactiveUrls, CacheDuration);
            }

            var isInactiveMenuUrl = inactiveUrls.Any(url => !string.IsNullOrEmpty(url) && path != null && path.Equals(url.ToLower().TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

            if (isInactiveMenuUrl)
            {
                _logger.LogWarning("Pasif menüye erişim engellendi: {Path}, Kullanıcı: {User}", path, context.User.Identity?.Name);
                context.Response.Redirect("/Account/AccessDenied");
                return;
            }
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

