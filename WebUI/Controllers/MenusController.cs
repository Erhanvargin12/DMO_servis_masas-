using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public MenusController(
            IMenuApiService menuApiService,
            IRoleApiService roleApiService,
            ILogger<MenusController> logger)
        {
            _menuApiService = menuApiService;
            _roleApiService = roleApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var menus = await _menuApiService.GetAllAsync();
            var roles = await _roleApiService.GetAllAsync();

            var model = new MenuListViewModel
            {
                Menus = menus,
                Roles = roles,
                TotalCount = menus.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] MenuViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new { success = false, message = "Menü adı zorunludur" });
            }

            bool success;
            string? message;

            if (model.Id == 0)
            {
                (success, message) = await _menuApiService.AddAsync(model);
            }
            else
            {
                (success, message) = await _menuApiService.UpdateAsync(model);
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _menuApiService.DeleteAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuRoles(int menuId)
        {
            var roles = await _menuApiService.GetMenuRolesAsync(menuId);
            return Json(new { success = true, data = roles });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRoles([FromBody] SaveMenuRolesRequest request)
        {
            if (request.MenuId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz menü" });
            }

            var (success, message) = await _menuApiService.UpdateMenuRolesAsync(request.MenuId, request.RoleIds ?? new List<int>());
            return Json(new { success, message });
        }
    }

    public class SaveMenuRolesRequest
    {
        public int MenuId { get; set; }
        public List<int>? RoleIds { get; set; }
    }

    public class MenuListViewModel
    {
        public List<MenuViewModel> Menus { get; set; } = new();
        public List<RoleViewModel> Roles { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

