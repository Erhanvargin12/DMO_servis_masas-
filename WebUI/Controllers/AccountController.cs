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

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
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
                
                // UserId claim'ini bul (nameidentifier veya sub)
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

                // Kullanıcı adı claim'ini bul
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" ||
                    c.Type == ClaimTypes.Name ||
                    c.Type == "name");
                
                if (nameClaim != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                }

                // Rol claim'lerini ekle
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            // Session temizle
            HttpContext.Session.Clear();

            // Cookie'den çıkış yap
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("Kullanıcı çıkış yaptı");

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
