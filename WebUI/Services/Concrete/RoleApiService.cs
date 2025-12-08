using Newtonsoft.Json;
using System.Text;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete
{
    public class RoleApiService : IRoleApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RoleApiService> _logger;

        public RoleApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<RoleApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<RoleViewModel>> GetAllAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("operationclaim/getall");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("GetAll Roles Response: {StatusCode} - {Content}", response.StatusCode, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiDataResponse<List<RoleViewModel>>>(content);
                    return result?.Data ?? new List<RoleViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Roller getirilirken hata oluştu");
            }
            return new List<RoleViewModel>();
        }

        public async Task<RoleViewModel?> GetByIdAsync(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"operationclaim/getbyid/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiDataResponse<RoleViewModel>>(content);
                    return result?.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol getirilirken hata oluştu: {Id}", id);
            }
            return null;
        }

        public async Task<(bool Success, string? Message)> AddAsync(RoleViewModel role)
        {
            try
            {
                SetAuthHeader();
                
                var addDto = new
                {
                    Id = 0,
                    role.Name,
                    Description = role.Description ?? ""
                };
                
                var json = JsonConvert.SerializeObject(addDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("operationclaim/add", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Add Role Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Rol başarıyla eklendi");
                }

                if (!string.IsNullOrEmpty(responseContent) && responseContent.TrimStart().StartsWith("{"))
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                    return (false, result?.Message ?? "Bir hata oluştu");
                }
                
                return (false, responseContent ?? "Bir hata oluştu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol eklenirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateAsync(RoleViewModel role)
        {
            try
            {
                SetAuthHeader();
                
                var updateDto = new
                {
                    role.Id,
                    role.Name,
                    Description = role.Description ?? ""
                };
                
                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("operationclaim/update", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Update Role Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Rol başarıyla güncellendi");
                }

                if (!string.IsNullOrEmpty(responseContent) && responseContent.TrimStart().StartsWith("{"))
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                    return (false, result?.Message ?? "Bir hata oluştu");
                }
                
                return (false, responseContent ?? "Bir hata oluştu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol güncellenirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> DeleteAsync(int id)
        {
            try
            {
                SetAuthHeader();
                
                // Direkt ID ile sil - GetByIdAsync kullanma (tracking sorunu)
                var deleteDto = new
                {
                    Id = id,
                    Name = "",
                    Description = ""
                };
                
                var json = JsonConvert.SerializeObject(deleteDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("operationclaim/delete", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Delete Role Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Rol başarıyla silindi");
                }

                if (!string.IsNullOrEmpty(responseContent) && responseContent.TrimStart().StartsWith("{"))
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                    return (false, result?.Message ?? "Silme işlemi başarısız");
                }
                
                return (false, responseContent ?? "Silme işlemi başarısız");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol silinirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }
    }
}
