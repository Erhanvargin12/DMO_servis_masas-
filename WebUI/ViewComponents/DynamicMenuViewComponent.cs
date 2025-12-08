using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.ViewComponents;

public class DynamicMenuViewComponent : ViewComponent
{
    private readonly IMenuApiService _menuApiService;

    public DynamicMenuViewComponent(IMenuApiService menuApiService)
    {
        _menuApiService = menuApiService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = GetCurrentUserId();
        
        List<MenuViewModel> menus;
        
        if (userId > 0)
        {
            // Kullanıcının yetkilerine göre menüleri getir
            menus = await _menuApiService.GetMenusByUserIdAsync(userId);
        }
        else
        {
            // Yetkisiz kullanıcı - boş menü
            menus = new List<MenuViewModel>();
        }

        return View(menus);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }
}

