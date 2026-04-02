using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;

namespace WebUI.Core.Middleware
{
    /// <summary>
    /// Access token süresi dolmadan önce otomatik yenileme middleware'ı.
    /// Token'ın son 5 dakikasına girildiyse yenileme işlemi başlatır.
    /// </summary>
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRefreshMiddleware> _logger;
        
        // Token yenileme için kalan süre eşiği (dakika)
        private const int RefreshThresholdMinutes = 5;

        public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Auth sayfalarında token kontrolü yapma
            if (IsAuthPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Kullanıcı giriş yapmamışsa devam et
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            try
            {
                var accessToken = context.Session.GetString("AccessToken");
                var tokenExpirationStr = context.Session.GetString("TokenExpiration");

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(tokenExpirationStr))
                {
                    if (DateTime.TryParse(tokenExpirationStr, out var tokenExpiration))
                    {
                        var timeUntilExpiration = tokenExpiration - DateTime.UtcNow;

                        // Token süresi dolmadan 5 dakika kala yenile
                        if (timeUntilExpiration.TotalMinutes <= RefreshThresholdMinutes && timeUntilExpiration.TotalMinutes > 0)
                        {
                            _logger.LogInformation("Token yenileniyor. Kalan süre: {Minutes} dakika", timeUntilExpiration.TotalMinutes);
                            
                            //var (success, newToken) = await authService.RefreshTokenAsync();
                            
                            //if (success && newToken != null)
                            //{
                            //    // Session'ı güncelle
                            //    context.Session.SetString("AccessToken", newToken.Token);
                            //    context.Session.SetString("TokenExpiration", newToken.Expiration.ToString("O"));
                                
                            //    // Cookie claim'lerini güncelle
                            //    await UpdateAuthCookie(context, newToken);
                                
                            //    _logger.LogInformation("Token başarıyla yenilendi");
                            //}
                            //else
                            //{
                            //    _logger.LogWarning("Token yenileme başarısız");
                            //}
                        }
                        // Token süresi dolmuşsa çıkış yap
                        else if (timeUntilExpiration.TotalMinutes <= 0)
                        {
                            _logger.LogWarning("Token süresi dolmuş, kullanıcı çıkış yapılıyor");
                            
                            context.Session.Clear();
                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            context.Response.Redirect("/Account/Login?expired=true");
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

        private bool IsAuthPath(PathString path)
        {
            var authPaths = new[] { "/Account/Login", "/Account/Register", "/Account/ForgotPassword", "/Account/ResetPassword" };
            return authPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }

        private async Task UpdateAuthCookie(HttpContext context, AccessTokenResponse token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.Token);

                var claims = new List<Claim>();
                
                // Mevcut claim'leri koru
                foreach (var claim in context.User.Claims)
                {
                    if (claim.Type != "AccessToken")
                    {
                        claims.Add(claim);
                    }
                }
                
                // Yeni token'ı ekle
                claims.Add(new Claim("AccessToken", token.Token));

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
                // Loglama yap ama devam et
                System.Diagnostics.Debug.WriteLine($"Auth cookie güncelleme hatası: {ex.Message}");
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
}

