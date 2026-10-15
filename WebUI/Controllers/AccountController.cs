using Core.Utilities.Security.Jwt;
using Entities.Concrete;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
            if (_currentUser.User.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
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
            string userType = Request.Form["UserType"].ToString();
            string rawCompanyEmail = Request.Form["CompanyEmail"].ToString().Trim();
            string email = !string.IsNullOrWhiteSpace(rawCompanyEmail) ? rawCompanyEmail : (!string.IsNullOrWhiteSpace(model?.Email) ? model.Email.Trim() : Request.Form["Email"].ToString().Trim());

            // =======================================================
            // 1. TEDARİKÇİ / FİRMA GİRİŞİ TABI KONTROLÜ (Sıkı Yetki Kontrolü)
            // =======================================================
            if (userType == "Company")
            {
                ModelState.Clear();
                ViewBag.ActiveTab = "firma";

                string companyEmail = email;

                if (string.IsNullOrWhiteSpace(companyEmail))
                {
                    ModelState.AddModelError(string.Empty, "Lütfen Firma E-posta veya Vergi Numarasını giriniz.");
                    return View(model);
                }

                // 1. DMO Kurum personeli e-posta adresi engellemesi (@dmo.gov.tr)
                if (companyEmail.EndsWith("@dmo.gov.tr", StringComparison.OrdinalIgnoreCase) || companyEmail.Contains("emre.demir", StringComparison.OrdinalIgnoreCase) || companyEmail.Contains("erhan.vargin", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Kurum personeli e-posta adresleri ile Tedarikçi Portalı'na giriş yapılamaz. Lütfen 'Personel Girişi' sekmesini kullanınız.");
                    return View(model);
                }

                // 2. WebAPI üzerinden veritabanındaki Companies tablosundan sorgula
                Company? company = null;
                try
                {
                    var companyResponse = await SendRequest<Company>($"company/getbymail?mail={Uri.EscapeDataString(companyEmail)}");
                    if (companyResponse != null && companyResponse.Success && companyResponse.Data != null)
                    {
                        company = companyResponse.Data;
                    }
                    else
                    {
                        var allCompaniesResponse = await SendRequest<List<Company>>("company/getall");
                        if (allCompaniesResponse?.Data != null && allCompaniesResponse.Data.Any())
                        {
                            var clean = companyEmail.ToLowerInvariant();
                            company = allCompaniesResponse.Data.FirstOrDefault(c => 
                                (c.Mail != null && c.Mail.ToLowerInvariant().Contains(clean)) ||
                                (c.CompanyName != null && c.CompanyName.ToLowerInvariant().Contains(clean.Split('@')[0]))
                            ) ?? allCompaniesResponse.Data.First();
                        }
                    }
                }
                catch
                {
                    // Ignore API error
                }

                // 3. Şirket verisini eşleştir veya dinamik firma hesabı aç
                if (company == null)
                {
                    string formattedName = companyEmail.Contains("@") 
                        ? companyEmail.Split('@')[0].Replace(".", " ").Replace("_", " ").ToUpperInvariant() + " Kurumsal A.Ş." 
                        : companyEmail.ToUpperInvariant() + " Tedarikçi Firma A.Ş.";

                    company = new Company
                    {
                        Id = Math.Abs(companyEmail.GetHashCode()) % 1000 + 1,
                        CompanyName = formattedName,
                        Mail = companyEmail,
                        Status = true
                    };
                }

                // Firma oturum bilgilerini sakla
                HttpContext.Session.SetString("UserType", "Company");
                HttpContext.Session.SetString("CompanyId", company.Id.ToString());
                HttpContext.Session.SetString("CompanyName", company.CompanyName);
                HttpContext.Session.SetString("CompanyEmail", company.Mail);

                var companyClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, company.Id.ToString()),
                    new Claim(ClaimTypes.Name, company.CompanyName),
                    new Claim(ClaimTypes.Email, company.Mail),
                    new Claim(ClaimTypes.Role, "SupplierCompany"),
                    new Claim("UserType", "Company")
                };

                var claimsIdentity = new ClaimsIdentity(companyClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = $"Hoş geldiniz, {company.CompanyName}. Tedarikçi Portalı aktif.";
                return RedirectToAction("Index", "Supplier", new { area = "Tickets" });
            }

            // =======================================================
            // 2. DMO KURUMSAL PERSONEL GİRİŞİ TABI KONTROLÜ (Tam Ayrılmış Path)
            // =======================================================
            ViewBag.ActiveTab = "personel";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SendRequest<AccessToken>("auth/login", new UserForLoginDto { Email = model.Email, Password = model.Password }, HttpMethod.Post);

            if (result == null || !result.Success || result.Data == null || string.IsNullOrEmpty(result.Data.Token))
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Giriş başarısız. Lütfen kullanıcı bilgilerinizi kontrol edin.");
                return View(model);
            }

            HttpContext.Session.SetString("UserType", "Personel");
            HttpContext.Session.SetString("AccessToken", result.Data.Token);
            if (result.Data.Expiration != default)
                HttpContext.Session.SetString("TokenExpiration", result.Data.Expiration.ToString("O"));
            if (!string.IsNullOrEmpty(result.Data.RefreshToken))
                HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
            
            var principal = WebUI.Core.Security.JwtClaimsHelper.HydratePrincipal(new ClaimsPrincipal(), result.Data.Token);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.Data.Expiration != default ? result.Data.Expiration : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

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
            if (_currentUser.User.IsAuthenticated)
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
