using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
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

            var (success, message, token) = await _authService.LoginAsync(model.Email, model.Password);

            if (!success || token == null)
            {
                ModelState.AddModelError(string.Empty, message ?? "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.");
                return View(model);
            }

            // Token'ı session'a kaydet
            HttpContext.Session.SetString("AccessToken", token.Token);
            HttpContext.Session.SetString("TokenExpiration", token.Expiration.ToString("O"));

            // JWT Token'dan claim'leri oku
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("AccessToken", token.Token)
            };

            // JWT token'dan UserId ve diğer claim'leri çıkar
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.Token);
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == "sub" ||
                    c.Type == "nameid");
                
                if (userIdClaim != null)
                {
                    claims.Add(new Claim("UserId", userIdClaim.Value));
                    HttpContext.Session.SetString("UserId", userIdClaim.Value);
                }

                var nameClaim = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" ||
                    c.Type == ClaimTypes.Name ||
                    c.Type == "name");
                
                if (nameClaim != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                }

                var roleClaims = jwtToken.Claims.Where(c => 
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                    c.Type == ClaimTypes.Role ||
                    c.Type == "role");
                
                foreach (var roleClaim in roleClaims)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token parse edilemedi");
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = token.Expiration
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);

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
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, token) = await _authService.RegisterAsync(
                model.Email, 
                model.Password, 
                model.FirstName, 
                model.LastName);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message ?? "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.");
                return View(model);
            }

            _logger.LogInformation("Yeni kullanıcı kaydoldu: {Email}", model.Email);

            // Başarılı kayıt sonrası login sayfasına yönlendir
            TempData["RegisterSuccess"] = "Hesabınız başarıyla oluşturuldu. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Not: Şifre sıfırlama için backend'de email gönderimi yapılmalı
            // Şimdilik sadece bilgi mesajı gösteriyoruz
            _logger.LogInformation("Şifre sıfırlama talebi: {Email}", model.Email);

            model.IsSubmitted = true;
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Not: Backend'de şifre sıfırlama endpoint'i implemente edilmeli
            // Şimdilik başarılı kabul ediyoruz
            _logger.LogInformation("Şifre sıfırlandı: {Email}", model.Email);

            TempData["PasswordResetSuccess"] = "Şifreniz başarıyla değiştirildi. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("Kullanıcı çıkış yaptı");

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
