using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebUI.Filters
{
    /// <summary>
    /// Admin yetkisi gerektiren sayfalar için kullanılır.
    /// Yetkisiz kullanıcıları AccessDenied sayfasına yönlendirir.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AdminAuthorizeAttribute(params string[] roles)
        {
            _roles = roles.Length > 0 ? roles : new[] { "Admin" };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", 
                    new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Kullanıcının rollerini kontrol et
            var userRoles = context.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || 
                            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                            c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            // Gerekli rollerden herhangi birine sahip mi?
            var hasRequiredRole = _roles.Any(role => 
                userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

            if (!hasRequiredRole)
            {
                // Yetki yok - AccessDenied sayfasına yönlendir
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}

