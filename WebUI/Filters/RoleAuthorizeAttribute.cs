using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebUI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _requiredRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Login kontrolü
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login","Account",new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Token içindeki role claim'leri
            var userRoles = context.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            // En az bir rol yeterli mi?
            var hasRole = _requiredRoles.Any(r =>userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

            if (!hasRole)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }

}
