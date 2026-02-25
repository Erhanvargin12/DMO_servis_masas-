using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class UsersController : Controller
    {
        private const int DefaultPageSize = 10;

        private readonly IUserApiService _userApiService;
        private readonly IRoleApiService _roleApiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserApiService userApiService,
            IRoleApiService roleApiService,
            ILogger<UsersController> logger)
        {
            _userApiService = userApiService;
            _roleApiService = roleApiService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Listeleme + Server-side Pagination
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var allUsers = await _userApiService.GetAllAsync();
            var allRoles = await _roleApiService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                allUsers = allUsers.Where(u =>
                    u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Mail.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var paged = PagedResult<UserViewModel>.Create(allUsers, page, DefaultPageSize);

            // Mevcut sayfa kullanıcıları için rolleri sıralı çek (HttpClient thread-safety)
            foreach (var user in paged.Items)
            {
                var roles = await _userApiService.GetUserRolesAsync(user.Id);
                user.AssignedRoleIds = roles.Select(r => r.OperationClaimId).ToList();
            }

            var model = new UserListViewModel
            {
                Users = paged.Items,
                AllRoles = allRoles,
                SearchTerm = search,
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };

            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CRUD (Create / Update / Delete)
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UserViewModel model)
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
                (success, message) = await _userApiService.AddAsync(model);
            else
                (success, message) = await _userApiService.UpdateAsync(model);

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
                TempData["ErrorMessage"] = "Geçersiz kullanıcı";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _userApiService.DeleteAsync(id);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Rol Atama (Modal üzerinden POST, ayrı sayfa yok)
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(int userId, List<int>? selectedRoleIds)
        {
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _userApiService.UpdateUserRolesAsync(
                userId, selectedRoleIds ?? new List<int>());

            if (success)
                TempData["SuccessMessage"] = message ?? "Roller güncellendi";
            else
                TempData["ErrorMessage"] = message ?? "Bir hata oluştu";

            return RedirectToAction(nameof(Index));
        }
    }
}
