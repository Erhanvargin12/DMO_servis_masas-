using WebUI.Models.ViewModels;

namespace WebUI.Services.Abstract;

public interface ICurrentUserService
{
    UserContext User { get; }

}
