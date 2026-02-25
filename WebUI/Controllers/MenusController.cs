using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebUI.Filters;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class MenusController : Controller
    {
        private readonly IMenuApiService _menuApiService;
        private readonly IRoleApiService _roleApiService;
        private readonly ILogger<MenusController> _logger;
        private readonly IMemoryCache _cache;

        public MenusController(
            IMenuApiService menuApiService,
            IRoleApiService roleApiService,
            ILogger<MenusController> logger,
            IMemoryCache cache)
        {
            _menuApiService = menuApiService;
            _roleApiService = roleApiService;
            _logger = logger;
            _cache = cache;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Listeleme
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var menus = await _menuApiService.GetAllAsync();
            var roles = await _roleApiService.GetAllAsync();

            // Tüm menüler için rolleri sıralı çek (HttpClient thread-safety)
            foreach (var menu in menus)
            {
                menu.AssignedRoleIds = await _menuApiService.GetMenuRolesAsync(menu.Id);
            }

            var model = new MenuListViewModel
            {
                Menus = menus,
                Roles = roles,
                TotalCount = menus.Count
            };

            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CRUD
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MenuViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                TempData["ErrorMessage"] = "Menü adı zorunludur";
                return RedirectToAction(nameof(Index));
            }

            bool success;
            string? message;

            if (model.Id == 0)
                (success, message) = await _menuApiService.AddAsync(model);
            else
                (success, message) = await _menuApiService.UpdateAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                _cache.Remove("InactiveMenuUrls");
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _menuApiService.DeleteAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                _cache.Remove("InactiveMenuUrls");
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Rol Atama (Modal üzerinden POST, ayrı sayfa yok)
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(int menuId, List<int>? selectedRoleIds)
        {
            if (menuId <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz menü";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _menuApiService.UpdateMenuRolesAsync(
                menuId, selectedRoleIds ?? new List<int>());

            if (success)
            {
                TempData["SuccessMessage"] = message ?? "Roller güncellendi";
                _cache.Remove("InactiveMenuUrls");
            }
            else
            {
                TempData["ErrorMessage"] = message ?? "Bir hata oluştu";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
