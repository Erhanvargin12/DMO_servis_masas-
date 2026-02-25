using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class RolesController : Controller
    {
        private const int DefaultPageSize = 10;

        private readonly IRoleApiService _roleApiService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleApiService roleApiService, ILogger<RolesController> logger)
        {
            _roleApiService = roleApiService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Listeleme + Server-side Pagination
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var allRoles = await _roleApiService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                allRoles = allRoles.Where(r =>
                    r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            var paged = PagedResult<RoleViewModel>.Create(allRoles, page, DefaultPageSize);

            var model = new RoleListViewModel
            {
                Roles = paged.Items,
                SearchTerm = search,
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };

            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CRUD
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            bool success;
            string? message;

            if (model.Id == 0)
                (success, message) = await _roleApiService.AddAsync(model);
            else
                (success, message) = await _roleApiService.UpdateAsync(model);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz rol";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _roleApiService.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
