//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using WebUI.Models;

//namespace WebUI.Controllers
//{
//    [Authorize]
//    public class ProfileController : Controller
//    {
//        private readonly ILogger<ProfileController> _logger;

//        public ProfileController(ILogger<ProfileController> logger)
//        {
//            _logger = logger;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var userId = GetCurrentUserId();
//            if (userId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var user = await _userApiService.GetByIdAsync(userId);
//            if (user == null)
//            {
//                TempData["ErrorMessage"] = "Kullanıcı bilgileri yüklenemedi";
//                return RedirectToAction("Index", "Home");
//            }

//            var model = new ProfileViewModel
//            {
//                Id = user.Id,
//                FirstName = user.FirstName,
//                LastName = user.LastName,
//                Mail = user.Mail,
//                CreateAt = DateTime.Now,
//            };

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> UpdateInfo(UserViewModel model)
//        {
//            var userId = GetCurrentUserId();
//            if (userId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            model.Id = userId;

//            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
//            {
//                TempData["ErrorMessage"] = "Ad ve soyad zorunludur";
//                return RedirectToAction(nameof(Index));
//            }

//            var existing = await _userApiService.GetByIdAsync(userId);
//            if (existing != null)
//            {
//                model.Status = existing.Status;
//                model.RegistirationId = existing.RegistirationId;
//                model.OrganizationUnitId = existing.OrganizationUnitId;
//                model.PositionId = existing.PositionId;
//            }

//            var (success, message) = await _userApiService.UpdateAsync(model);

//            if (success)
//                TempData["SuccessMessage"] = "Profil bilgileri güncellendi";
//            else
//                TempData["ErrorMessage"] = message;

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
//                return RedirectToAction(nameof(Index));
//            }

//            var userId = GetCurrentUserId();
//            if (userId == 0)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var (success, message) = await _userApiService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

//            if (success)
//                TempData["SuccessMessage"] = message;
//            else
//                TempData["ErrorMessage"] = message;

//            return RedirectToAction(nameof(Index));
//        }

//        private int GetCurrentUserId()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
//                           ?? User.FindFirst("UserId")?.Value;

//            if (int.TryParse(userIdClaim, out int userId))
//            {
//                return userId;
//            }
//            return 0;
//        }
//    }
//}

