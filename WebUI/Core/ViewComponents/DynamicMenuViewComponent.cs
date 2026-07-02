using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebUI.Core.Services.Abstract;
using WebUI.Models.DTOs;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;

namespace WebUI.Core.ViewComponents;

public class DynamicMenuViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApiClient _apiClient;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DynamicMenuViewComponent(ICurrentUserService currentUserService, IApiClient apiClient, IMemoryCache cache)
    {
        _currentUserService = currentUserService;
        _apiClient = apiClient;
        _cache = cache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _currentUserService.User.UserId;
        var menus = new MenuViewModel();

        if (userId == 0)
            return View(menus);

        var cacheKey = $"UserMenu_{userId}";

        if (!_cache.TryGetValue(cacheKey, out List<MenuDto>? cachedMenu) || cachedMenu == null)
        {
            var token = HttpContext.User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                _apiClient.SetBearerToken(token);

            var userMenu = await _apiClient.GetAsync<List<MenuDto>>($"menu/get-user-menus/{userId}");
            cachedMenu = userMenu.Data ?? new List<MenuDto>();

            _cache.Set(cacheKey, cachedMenu, CacheDuration);
        }

        menus.Menu = cachedMenu;
        return View(menus);
    }
}