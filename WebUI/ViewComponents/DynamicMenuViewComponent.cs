using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.ViewComponents;

public class DynamicMenuViewComponent : ViewComponent
{
    private readonly IMenuApiService _menuApiService;
    private readonly ICurrentUserService _currentUserService;

    public DynamicMenuViewComponent(IMenuApiService menuApiService, ICurrentUserService currentUserService)
    {
        _menuApiService = menuApiService;
        _currentUserService = currentUserService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _currentUserService.UserId;

        List<MenuViewModel> menus;

        if (!string.IsNullOrEmpty(userId))
        {
            menus = await _menuApiService.GetMenusByUserIdAsync(userId);
        }
        else
        {
            menus = new List<MenuViewModel>();
        }

        return View(menus);
    }
}