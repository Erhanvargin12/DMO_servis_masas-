using WebUI.Models;

namespace WebUI.Services.Abstract;

public interface IMenuApiService
{
    Task<List<MenuViewModel>> GetMenusByUserIdAsync(string userId);
    Task<List<MenuViewModel>> GetAllAsync();
    Task<MenuViewModel?> GetByIdAsync(int id);
    Task<(bool Success, string? Message)> AddAsync(MenuViewModel menu);
    Task<(bool Success, string? Message)> UpdateAsync(MenuViewModel menu);
    Task<(bool Success, string? Message)> DeleteAsync(int id);
    
    // Rol işlemleri
    Task<List<int>> GetMenuRolesAsync(int menuId);
    Task<(bool Success, string? Message)> UpdateMenuRolesAsync(int menuId, List<int> roleIds);
}
