using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using WebUI.Services.Abstract;

namespace WebUI.Middleware
{
    public class MenuAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MenuAccessMiddleware> _logger;
        private const string CacheKey = "InactiveMenuUrls";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public MenuAccessMiddleware(RequestDelegate next, ILogger<MenuAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IMenuApiService menuApiService, IMemoryCache cache)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Statik dosyalar, auth sayfaları ve API çağrılarını atla
            if (ShouldSkip(path))
            {
                await _next(context);
                return;
            }

            // Sadece giriş yapmış kullanıcılar için kontrol et
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // Admin kullanıcılar her şeye erişebilir
            var isAdmin = context.User.Claims
                .Any(c => c.Type == ClaimTypes.Role &&
                          c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                await _next(context);
                return;
            }

            // Pasif menü URL'lerini cache'den al veya API'den çek
            if (!cache.TryGetValue(CacheKey, out List<string>? inactiveUrls) || inactiveUrls == null)
            {
                inactiveUrls = await menuApiService.GetInactiveUrlsAsync();
                cache.Set(CacheKey, inactiveUrls, CacheDuration);
            }

            // Mevcut path pasif bir menü URL'si ile eşleşiyor mu?
            var isInactiveMenuUrl = inactiveUrls.Any(url =>
                !string.IsNullOrEmpty(url) &&
                path != null &&
                path.Equals(url.ToLower().TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

            if (isInactiveMenuUrl)
            {
                _logger.LogWarning("Pasif menüye erişim engellendi: {Path}, Kullanıcı: {User}",
                    path, context.User.Identity?.Name);
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

