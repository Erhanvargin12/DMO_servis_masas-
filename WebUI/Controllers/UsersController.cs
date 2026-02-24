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

        public async Task<IActionResult> Index(string? search)
        {
            var users = await _userApiService.GetAllAsync();
            var roles = await _roleApiService.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u =>
                    u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Mail.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var model = new UserListViewModel
            {
                Users = users,
                Roles = roles,
                SearchTerm = search,
                TotalCount = users.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] UserViewModel model)
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
                (success, message) = await _userApiService.AddAsync(model);
            }
            else
            {
                (success, message) = await _userApiService.UpdateAsync(model);
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            if (request?.Id <= 0)
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı" });
            }

            var (success, message) = await _userApiService.DeleteAsync(request.Id);
            return Json(new { success, message });
        }

        public class DeleteRequest
        {
            public int Id { get; set; }
        }

        #region Rol Yönetimi

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var userRoles = await _userApiService.GetUserRolesAsync(userId);
            var roleIds = userRoles.Select(r => r.OperationClaimId).ToList();
            return Json(new { success = true, data = roleIds });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRoles([FromBody] SaveUserRolesRequest request)
        {
            if (request.UserId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı" });
            }

            var (success, message) = await _userApiService.UpdateUserRolesAsync(request.UserId, request.RoleIds ?? new List<int>());
            return Json(new { success, message });
        }

        #endregion
    }

    public class SaveUserRolesRequest
    {
        public int UserId { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
