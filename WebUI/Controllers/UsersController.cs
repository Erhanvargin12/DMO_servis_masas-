using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebUI.Core.Filters;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;

namespace WebUI.Controllers
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class UsersController : BaseController
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger, IApiClient apiClient) : base(apiClient)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var allUsers = await SendRequest<List<User>>("user/getall");
            var allRoles = await SendRequest<List<OperationClaim>>("operationclaim/getall");
            var userRoles = await SendRequest<List<UserOperationClaim>>("useroperationclaim/getall");

            var users = allUsers.Data.Select(u => new UserManagementViewModel
            {
                User = u,
                UserRoles = userRoles.Data.Where(ur => ur.UserId == u.Id).ToList()
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u => u.User.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         u.User.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         u.User.Mail.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var model = new UserListViewModel
            {
                Users = users,
                AllRoles = allRoles.Data,
                SearchTerm = search,
                TotalCount = users.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UserViewModel user)
        {
            IDataResult<User> result;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            result = await SendRequest<User>(user.Id == 0 ? "user/add" : "user/update", user, HttpMethod.Post);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
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

            var entity = await SendRequest<User>($"user/getbyid/{id}");
            var result = await SendRequest<List<User>>($"user/delete", entity.Data, HttpMethod.Post);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(int userId, List<int>? selectedRoleIds)
        {
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await SendRequest<List<UserOperationClaim>>($"useroperationclaim/getbyuserid/{userId}", HttpMethod.Get);

            foreach (var roleId in selectedRoleIds)
            {
                if (!userRoles.Data.Any(x => x.OperationClaimId == roleId))
                {
                    var userClaim = new UserOperationClaim { UserId = userId, OperationClaimId = roleId };
                    var result = await SendRequest<UserOperationClaim>("useroperationclaim/add", userClaim, HttpMethod.Post);

                    TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
                }
            }
            foreach (var role in userRoles.Data)
            {
                if (!selectedRoleIds.Contains(role.OperationClaimId))
                {
                    var userClaim = new UserOperationClaim { Id = role.Id, UserId = userId, OperationClaimId = role.OperationClaimId };
                    var result = await SendRequest<int>($"useroperationclaim/delete", userClaim, HttpMethod.Post);

                    TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
