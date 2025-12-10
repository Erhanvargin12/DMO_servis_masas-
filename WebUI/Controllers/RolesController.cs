using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    [AdminAuthorize("Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleApiService _roleApiService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleApiService roleApiService, ILogger<RolesController> logger)
        {
            _roleApiService = roleApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var roles = await _roleApiService.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                roles = roles.Where(r =>
                    r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            var model = new RoleListViewModel
            {
                Roles = roles,
                SearchTerm = search,
                TotalCount = roles.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            bool success;
            string? message;

            if (model.Id == 0)
            {
                (success, message) = await _roleApiService.AddAsync(model);
            }
            else
            {
                (success, message) = await _roleApiService.UpdateAsync(model);
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteRoleRequest request)
        {
            if (request?.Id <= 0)
            {
                return Json(new { success = false, message = "Geçersiz rol" });
            }

            var (success, message) = await _roleApiService.DeleteAsync(request.Id);
            return Json(new { success, message });
        }
    }

    public class DeleteRoleRequest
    {
        public int Id { get; set; }
    }
}
