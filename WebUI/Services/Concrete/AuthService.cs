using Newtonsoft.Json;
using System.Text;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string? Message, AccessTokenResponse? Token)> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var token = JsonConvert.DeserializeObject<AccessTokenResponse>(responseContent);
                    return (true, null, token);
                }

                _logger.LogWarning("Login başarısız: {StatusCode} - {Content}", response.StatusCode, responseContent);
                
                // Hata mesajını parse et
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(responseContent);
                    return (false, errorResponse?.Message ?? "Giriş başarısız", null);
                }
                catch
                {
                    return (false, "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login sırasında hata oluştu");
                return (false, "Bağlantı hatası oluştu. Lütfen tekrar deneyin.", null);
            }
        }

        public async Task<(bool Success, string? Message, AccessTokenResponse? Token)> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                var registerData = new
                {
                    Mail = email,
                    Password = password,
                    FirstName = firstName,
                    LastName = lastName
                };
                var json = JsonConvert.SerializeObject(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var token = JsonConvert.DeserializeObject<AccessTokenResponse>(responseContent);
                    return (true, null, token);
                }

                _logger.LogWarning("Register başarısız: {StatusCode} - {Content}", response.StatusCode, responseContent);
                
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(responseContent);
                    return (false, errorResponse?.Message ?? "Kayıt başarısız", null);
                }
                catch
                {
                    return (false, "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register sırasında hata oluştu");
                return (false, "Bağlantı hatası oluştu. Lütfen tekrar deneyin.", null);
            }
        }

        public async Task<(bool Success, AccessTokenResponse? Token)> RefreshTokenAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return (false, null);
                }

                // Authorization header'a mevcut token'ı ekle
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("auth/refresh", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var newToken = JsonConvert.DeserializeObject<AccessTokenResponse>(responseContent);
                    return (true, newToken);
                }

                _logger.LogWarning("Token refresh başarısız: {StatusCode}", response.StatusCode);
                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh sırasında hata oluştu");
                return (false, null);
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync("auth/logout", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout sırasında hata oluştu");
                return false;
            }
        }

        private class ApiErrorResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }
}
