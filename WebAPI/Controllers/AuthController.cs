using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        // Cookie ayarları
        private const string RefreshTokenCookieName = "refreshToken";
        private const int RefreshTokenExpirationDays = 7;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = await _authService.LoginAsync(userForLoginDto);
            if (!userToLogin.Success)
            {
                return BadRequest("Giriş Hatası");
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.CreateAccessTokenAsync(userToLogin.Data, ipAddress);
            
            if (result.Success)
            {
                SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);
                return Ok(result);
            }

            return BadRequest(result.Message);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userDto)
        {
            var registerResult = await _authService.RegisterAsync(userDto);
            if (!registerResult.Success)
            {
                return BadRequest(new { Success = false, Message = registerResult.Message });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.CreateAccessTokenAsync(registerResult.Data, ipAddress);
            
            if (result.Success)
            {
                SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);
                
                return Ok(new
                {
                    Token = result.Data.Token,
                    Expiration = result.Data.Expiration
                });
            }

            return BadRequest(new { Success = false, Message = result.Message });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? dto = null)
        {
            // Refresh token: önce body'den, yoksa cookie'den al
            var refreshToken = dto?.RefreshToken ?? Request.Cookies[RefreshTokenCookieName];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { Success = false, Message = "Refresh token bulunamadı" });
            }

            // Access token: önce body'den, yoksa header'dan al
            var accessToken = dto?.AccessToken 
                ?? Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized(new { Success = false, Message = "Access token bulunamadı" });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(accessToken, refreshToken, ipAddress);
            
            if (result.Success)
            {
                SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);
                
                return Ok(new
                {
                    result.Data.Token,
                    result.Data.Expiration,
                    result.Data.RefreshToken,
                    result.Data.RefreshTokenExpiration
                });
            }

            DeleteRefreshTokenCookie();
            return Unauthorized(new { Success = false, Message = result.Message });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { Success = false, Message = "Refresh token bulunamadı" });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            
            DeleteRefreshTokenCookie();
            
            if (result.Success)
            {
                return Ok(new { Success = true, Message = result.Message });
            }

            return BadRequest(new { Success = false, Message = result.Message });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = GetIpAddress();
                await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            }

            DeleteRefreshTokenCookie();
            
            return Ok(new { Success = true, Message = "Çıkış başarılı" });
        }

        #region Private Methods

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // JavaScript erişemez (XSS koruması)
                Secure = true,    // Sadece HTTPS üzerinden
                SameSite = SameSiteMode.Strict,  // CSRF koruması
                Expires = expires,
                Path = "/api/auth"  // Sadece auth endpoint'lerinde gönderilir
            };

            Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
        }

        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(-1),
                Path = "/api/auth"
            };

            Response.Cookies.Append(RefreshTokenCookieName, "", cookieOptions);
        }

        private string? GetIpAddress()
        {
            // X-Forwarded-For header'ı (reverse proxy arkasında)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }
            
            // X-Real-IP header'ı (nginx)
            if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                return Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }

        #endregion
    }
}
