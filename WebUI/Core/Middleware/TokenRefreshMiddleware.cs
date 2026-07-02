using Core.Utilities.Security.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebUI.Core.Services.Abstract;

namespace WebUI.Core.Middleware
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        private const int RefreshThresholdMinutes = 5;

        public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsAuthPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            try
            {
                var accessToken = context.Session.GetString("AccessToken");
                var refreshToken = context.Session.GetString("RefreshToken");
                var tokenExpirationStr = context.Session.GetString("TokenExpiration");

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(tokenExpirationStr))
                {
                    if (DateTime.TryParse(tokenExpirationStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var tokenExpiration))
                    {
                        var timeUntilExpiration = tokenExpiration - DateTime.Now;

                        var shouldRefresh = timeUntilExpiration.TotalMinutes <= RefreshThresholdMinutes;

                        if (shouldRefresh && !string.IsNullOrEmpty(refreshToken))
                        {
                            _logger.LogInformation("Token yenileniyor. Kalan süre: {Minutes:F1} dakika", timeUntilExpiration.TotalMinutes);

                            var apiClient = context.RequestServices.GetRequiredService<IApiClient>();
                            apiClient.SetBearerToken(accessToken);

                            var refreshResult = await apiClient.PostAsync<AccessToken>("auth/refresh", new RefreshTokenRequestDto
                            {
                                AccessToken = accessToken,
                                RefreshToken = refreshToken
                            });

                            if (refreshResult.Success && refreshResult.Data != null)
                            {
                                context.Session.SetString("AccessToken", refreshResult.Data.Token);
                                context.Session.SetString("TokenExpiration", refreshResult.Data.Expiration.ToString("O"));
                                context.Session.SetString("RefreshToken", refreshResult.Data.RefreshToken);

                                await UpdateAuthCookie(context, refreshResult.Data);

                                _logger.LogInformation("Token başarıyla yenilendi.");
                            }
                            else
                            {
                                _logger.LogWarning("Token yenileme başarısız: {Message}", refreshResult.Message);

                                if (timeUntilExpiration.TotalMinutes <= 0)
                                {
                                    await SignOut(context);
                                    return;
                                }
                            }
                        }
                        else if (timeUntilExpiration.TotalMinutes <= 0)
                        {
                            _logger.LogWarning("Token süresi dolmuş ve refresh token yok, çıkış yapılıyor.");
                            await SignOut(context);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh middleware hatası");
            }

            await _next(context);
        }

        private async Task SignOut(HttpContext context)
        {
            context.Session.Clear();
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login?expired=true");
        }

        private bool IsAuthPath(PathString path)
        {
            var authPaths = new[] { "/Account/Login", "/Account/Register", "/Account/ForgotPassword", "/Account/ResetPassword" };
            return authPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }

        private async Task UpdateAuthCookie(HttpContext context, AccessToken token)
        {
            try
            {
                var claims = context.User.Claims
                    .Where(c => c.Type != "AccessToken")
                    .Append(new Claim("AccessToken", token.Token))
                    .ToList();

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = token.Expiration
                };

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth cookie güncelleme hatası");
            }
        }
    }

    public static class TokenRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenRefreshMiddleware>();
        }
    }

    internal class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
