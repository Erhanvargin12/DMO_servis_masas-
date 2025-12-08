using WebUI.Models;

namespace WebUI.Services.Abstract
{
    public interface IRoleApiService
    {
        Task<List<RoleViewModel>> GetAllAsync();
        Task<RoleViewModel?> GetByIdAsync(int id);
        Task<(bool Success, string? Message)> AddAsync(RoleViewModel role);
        Task<(bool Success, string? Message)> UpdateAsync(RoleViewModel role);
        Task<(bool Success, string? Message)> DeleteAsync(int id);
    }
}

