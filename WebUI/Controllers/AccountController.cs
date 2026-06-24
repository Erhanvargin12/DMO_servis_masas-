using Core.Utilities.Security.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.DTOs;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ICurrentUserService _currentUser;

        public AccountController(ICurrentUserService currentUser, IApiClient apiClient) : base(apiClient)
        {
            _currentUser = currentUser;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SendRequest<AccessToken>("auth/login",new UserForLoginDto { Email = model.Email, Password = model.Password}, HttpMethod.Post);

            if (!result.Success || result.Data.Token == null)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.");
                return View(model);
            }

            HttpContext.Session.SetString("AccessToken", result.Data.Token);
            HttpContext.Session.SetString("TokenExpiration", result.Data.Expiration.ToString("O"));
            HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("AccessToken", result.Data.Token)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(result.Data.Token);
                string url = "http://schemas.microsoft.com/ws/2008/06/identity/claims/";
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == url+"nameidentifier" ||c.Type == ClaimTypes.NameIdentifier ||c.Type == "sub" ||c.Type == "nameid");
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == url + "name" || c.Type == ClaimTypes.Name || c.Type == "name");
                var roleClaims = jwtToken.Claims.Where(c => c.Type == url + "role" || c.Type == ClaimTypes.Role || c.Type == "role");

                if (userIdClaim != null)
                {
                    claims.Add(new Claim("UserId", userIdClaim.Value));
                    HttpContext.Session.SetString("UserId", userIdClaim.Value);
                }

                var branchIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "organizationUnitId");
                if (branchIdClaim != null)
                {
                    HttpContext.Session.SetString("BranchId", branchIdClaim.Value);
                }
                
                if (nameClaim != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                }

                foreach (var roleClaim in roleClaims)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
            }
            catch (Exception ex)
            {
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = result.Data.Expiration
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            if (_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var (success, message, token) = await _authService.RegisterAsync(
        //        model.Email, 
        //        model.Password, 
        //        model.FirstName, 
        //        model.LastName);

        //    if (!success)
        //    {
        //        ModelState.AddModelError(string.Empty, message ?? "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.");
        //        return View(model);
        //    }

        //    _logger.LogInformation("Yeni kullanıcı kaydoldu: {Email}", model.Email);

        //    // Başarılı kayıt sonrası login sayfasına yönlendir
        //    TempData["RegisterSuccess"] = "Hesabınız başarıyla oluşturuldu. Şimdi giriş yapabilirsiniz.";
        //    return RedirectToAction("Login");
        //}

        #endregion

        #region Forgot Password

        //[HttpGet]
        //public IActionResult ForgotPassword()
        //{
        //    if (_currentUser.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    return View(new ForgotPasswordViewModel());
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    _logger.LogInformation("Şifre sıfırlama talebi: {Email}", model.Email);

        //    model.IsSubmitted = true;
        //    return View(model);
        //}

        //[HttpGet]
        //public IActionResult ResetPassword(string token, string email)
        //{
        //    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var model = new ResetPasswordViewModel
        //    {
        //        Token = token,
        //        Email = email
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Not: Backend'de şifre sıfırlama endpoint'i implemente edilmeli
        //    // Şimdilik başarılı kabul ediyoruz
        //    _logger.LogInformation("Şifre sıfırlandı: {Email}", model.Email);

        //    TempData["PasswordResetSuccess"] = "Şifreniz başarıyla değiştirildi. Şimdi giriş yapabilirsiniz.";
        //    return RedirectToAction("Login");
        //}

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        #endregion

        #region Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion
    }
}
