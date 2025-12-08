using WebUI.Models;

namespace WebUI.Services.Abstract
{
    public interface IAuthService
    {
        Task<(bool Success, string? Message, AccessTokenResponse? Token)> LoginAsync(string email, string password);
        Task<bool> LogoutAsync();
        Task<(bool Success, string? Message, AccessTokenResponse? Token)> RegisterAsync(string email, string password, string firstName, string lastName);
    }
}

