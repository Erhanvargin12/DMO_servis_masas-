using WebUI.Core.Security;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete
{
    public class CurrentUserService : ICurrentUserService
    {
        public UserContext User { get; }

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            var context = accessor.HttpContext;
            User = context != null ? JwtClaimsHelper.MapToUserContext(context) : new UserContext();
        }
    }
}
