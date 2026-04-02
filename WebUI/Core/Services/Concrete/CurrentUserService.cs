using System.Security.Claims;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
        public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("UserId")?.Value;
        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("emailaddress")?.Value;
        public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value?? User?.FindFirst("name")?.Value;

        public IReadOnlyList<string> Roles => User?.Claims
                .Where(c =>c.Type == ClaimTypes.Role ||c.Type == "role" ||c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();

        public bool HasRole(params string[] roles)
        {
            if (!IsAuthenticated) return false;

            return roles.Any(r => Roles.Contains(r, StringComparer.OrdinalIgnoreCase));
        }
    }


}
