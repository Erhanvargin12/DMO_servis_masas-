using WebUI.Models;

namespace WebUI.Services.Abstract
{
    public interface IUserApiService
    {
        Task<List<UserViewModel>> GetAllAsync();
        Task<UserViewModel?> GetByIdAsync(int id);
        Task<(bool Success, string? Message)> AddAsync(UserViewModel user);
        Task<(bool Success, string? Message)> UpdateAsync(UserViewModel user);
        Task<(bool Success, string? Message)> DeleteAsync(int id);
        
        Task<(bool Success, string? Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // Rol işlemleri
        Task<List<UserRoleViewModel>> GetUserRolesAsync(int userId);
        Task<(bool Success, string? Message)> AddRoleAsync(int userId, int roleId);
        Task<(bool Success, string? Message)> RemoveRoleAsync(int userId, int roleId);
        Task<(bool Success, string? Message)> UpdateUserRolesAsync(int userId, List<int> roleIds);
    }
}
