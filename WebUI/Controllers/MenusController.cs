using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.DTOs;
using WebUI.Models.ViewModels;

namespace WebUI.Controllers
{

    [Authorize]
    public class MenusController : BaseController
    {
        private readonly IMemoryCache _cache;

        public MenusController(IMemoryCache cache, IApiClient apiClient) : base(apiClient)
        {
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var menus = await SendRequest<List<MenuDto>>("menu/get-menu-details");
            var roles = await SendRequest<List<OperationClaim>>("operationclaim/getall");
            var claims = await SendRequest<List<MenuOperationClaim>>("menuoperationclaim/getall");

            var model = new MenuViewModel
            {
                Menu = menus?.Data ?? new List<MenuDto>(),
                Roles = roles?.Data ?? new List<OperationClaim>(),
                Orphans = new List<Menu>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Menu menu)
        {
            if (string.IsNullOrWhiteSpace(menu.Name))
            {
                TempData["ErrorMessage"] = "Menü adı zorunludur";
                return RedirectToAction(nameof(Index));
            }

            var result = await SendRequest<Menu>(menu.Id == 0 ? "menu/add" : "menu/update", menu, HttpMethod.Post);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (result.Success)
                _cache.Remove("InactiveMenuUrls");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result= await SendRequest<Menu>("menu/delete", new Menu {Id=id}, HttpMethod.Post);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (result.Success)
                _cache.Remove("InactiveMenuUrls");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(int menuId, List<int>? selectedRoleIds)
        {
            if (menuId <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz menü";
                return RedirectToAction(nameof(Index));
            }

            var result = await SendRequest<List<UserOperationClaim>>($"menuoperationclaim/update/", new MenuOperationClaimUpdateDto {MenuId = menuId, RoleIds = selectedRoleIds}, HttpMethod.Post);
            if (result.Success)
                _cache.Remove("InactiveMenuUrls");

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
