using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ICurrentUserService currentUser, IApiClient apiClient, ILogger<ProfileController> logger) : base(apiClient)
        {
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _currentUser.User.UserId;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userResult = await SendRequest<User>($"user/getbyid/{userId}");
            if (userResult == null || !userResult.Success || userResult.Data == null)
            {
                TempData["ErrorMessage"] = userResult?.Message ?? "Kullanıcı bilgileri yüklenemedi";
                return View(new ProfileViewModel());
            }

            var user = userResult.Data;
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail,
                CreateAt = user.CreateAt,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(ProfileViewModel model)
        {
            var userId = _currentUser.User.UserId;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            model.Id = userId;
            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
            {
                TempData["ErrorMessage"] = "Ad ve soyad zorunludur";
                return RedirectToAction(nameof(Index));
            }

            var existingResult = await SendRequest<User>($"user/getbyid/{userId}");
            var user = existingResult?.Data ?? new User();
            user.Id = userId;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Mail = model.Mail ?? user.Mail;

            var result = await SendRequest<User>("user/update", user, HttpMethod.Post);

            if (result.Success)
                TempData["SuccessMessage"] = "Profil bilgileri güncellendi";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            var userId = _currentUser.User.UserId;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var dto = new ChangePasswordDto
            {
                UserId = userId,
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            };

            var result = await SendRequest<object>("user/changepassword", dto, HttpMethod.Post);

            if (result.Success)
                TempData["SuccessMessage"] = result.Message ?? "Şifreniz başarıyla güncellendi";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
