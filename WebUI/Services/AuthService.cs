using Newtonsoft.Json;
using System.Text;
using WebUI.Models;

namespace WebUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
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
                return (false, responseContent, null);
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
                return (false, responseContent, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register sırasında hata oluştu");
                return (false, "Bağlantı hatası oluştu. Lütfen tekrar deneyin.", null);
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("auth/logout", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout sırasında hata oluştu");
                return false;
            }
        }
    }
}

