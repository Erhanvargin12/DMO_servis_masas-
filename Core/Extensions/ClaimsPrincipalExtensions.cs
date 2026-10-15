using System.Security.Claims;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static List<string> Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
            return result ?? new List<string>();
        }

        public static List<string> ClaimRoles(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                return new List<string>();

            var roles = claimsPrincipal.Claims(ClaimTypes.Role)
                .Concat(claimsPrincipal.Claims("role"))
                .Concat(claimsPrincipal.Claims("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return roles;
        }
    }
}