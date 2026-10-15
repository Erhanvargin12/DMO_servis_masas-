using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;

namespace WebUI.Controllers;

[Authorize]
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

        var userList = allUsers?.Data ?? new List<User>();
        var roleList = allRoles?.Data ?? new List<OperationClaim>();
        var claimList = userRoles?.Data ?? new List<UserOperationClaim>();

        var users = userList.Select(u => new UserManagementViewModel
        {
            User = u,
            UserRoles = claimList.Where(ur => ur.UserId == u.Id).ToList()
        }).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            users = users.Where(u => u.User != null && (
                (u.User.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.User.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.User.Mail?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
            )).ToList();
        }

        var model = new UserListViewModel
        {
            Users = users,
            AllRoles = roleList,
            SearchTerm = search,
            TotalCount = users.Count
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(UserViewModel user)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        IDataResult<User> result;
        if (user.Id == 0)
        {
            var addDto = new Entities.DTOs.UserAddDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail,
                Password = string.IsNullOrWhiteSpace(user.Password) ? "123456" : user.Password,
                Status = user.Status,
                EbysUserId = user.EbysUserId ?? string.Empty,
                BranchId = user.BranchId ?? string.Empty,
                PositionId = user.PositionId
            };
            result = await SendRequest<User>("user/add", addDto, HttpMethod.Post);
        }
        else
        {
            var updateDto = new Entities.DTOs.UserUpdateDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail,
                Status = user.Status,
                RegistirationId = user.EbysUserId ?? string.Empty,
                BranchId = user.BranchId ?? string.Empty,
                PositionId = user.PositionId
            };
            result = await SendRequest<User>("user/update", updateDto, HttpMethod.Post);
        }

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

        var userRoles = await SendRequest<List<UserOperationClaim>>($"useroperationclaim/getbyuserid/{userId}");
        var existingClaims = userRoles?.Data ?? new List<UserOperationClaim>();
        var roleIds = selectedRoleIds ?? new List<int>();

        foreach (var roleId in roleIds)
        {
            if (!existingClaims.Any(x => x.OperationClaimId == roleId))
            {
                var userClaim = new UserOperationClaim { UserId = userId, OperationClaimId = roleId };
                var result = await SendRequest<UserOperationClaim>("useroperationclaim/add", userClaim, HttpMethod.Post);

                TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            }
        }
        foreach (var role in existingClaims)
        {
            if (!roleIds.Contains(role.OperationClaimId))
            {
                var userClaim = new UserOperationClaim { Id = role.Id, UserId = userId, OperationClaimId = role.OperationClaimId };
                var result = await SendRequest<int>($"useroperationclaim/delete", userClaim, HttpMethod.Post);

                TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            }
        }
        return RedirectToAction(nameof(Index));
    }
}
