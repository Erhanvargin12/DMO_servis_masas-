using Newtonsoft.Json;
using System.Text;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete
{
    public class UserApiService : IUserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserApiService> _logger;

        public UserApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<UserApiService> logger)
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

        public async Task<List<UserViewModel>> GetAllAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("user/getall");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("GetAll Response: {StatusCode} - {Content}", response.StatusCode, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiDataResponse<List<UserViewModel>>>(content);
                    return result?.Data ?? new List<UserViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcılar getirilirken hata oluştu");
            }
            return new List<UserViewModel>();
        }

        public async Task<UserViewModel?> GetByIdAsync(int id)
        {
            var users = await GetAllAsync();
            return users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<(bool Success, string? Message)> AddAsync(UserViewModel user)
        {
            try
            {
                SetAuthHeader();
                
                var addDto = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Mail,
                    Password = "123456",
                    user.Status,
                    user.RegistirationId,
                    user.OrganizationUnitId,
                    user.PositionId
                };
                
                var json = JsonConvert.SerializeObject(addDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("user/add", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Add Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kullanıcı başarıyla eklendi");
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
                _logger.LogError(ex, "Kullanıcı eklenirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateAsync(UserViewModel user)
        {
            try
            {
                SetAuthHeader();
                
                var updateDto = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Mail,
                    user.Status,
                    user.RegistirationId,
                    user.OrganizationUnitId,
                    user.PositionId
                };
                
                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("user/update", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Update Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kullanıcı başarıyla güncellendi");
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
                _logger.LogError(ex, "Kullanıcı güncellenirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> DeleteAsync(int id)
        {
            try
            {
                SetAuthHeader();
                
                var deleteDto = new { Id = id };
                var json = JsonConvert.SerializeObject(deleteDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("user/delete", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Delete Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kullanıcı başarıyla silindi");
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
                _logger.LogError(ex, "Kullanıcı silinirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        #region Rol İşlemleri

        public async Task<List<UserRoleViewModel>> GetUserRolesAsync(int userId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"useroperationclaim/getbyuserid/{userId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiDataResponse<List<UserRoleViewModel>>>(content);
                    return result?.Data ?? new List<UserRoleViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı rolleri getirilirken hata oluştu");
            }
            return new List<UserRoleViewModel>();
        }

        public async Task<(bool Success, string? Message)> AddRoleAsync(int userId, int roleId)
        {
            try
            {
                SetAuthHeader();
                
                var dto = new { UserId = userId, OperationClaimId = roleId };
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("useroperationclaim/add", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Rol başarıyla atandı");
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
                _logger.LogError(ex, "Rol atanırken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> RemoveRoleAsync(int userId, int roleId)
        {
            try
            {
                SetAuthHeader();
                
                // Önce kullanıcının bu rolünü bul
                var userRoles = await GetUserRolesAsync(userId);
                var roleToRemove = userRoles.FirstOrDefault(r => r.OperationClaimId == roleId);
                
                if (roleToRemove == null)
                {
                    return (false, "Rol bulunamadı");
                }

                var dto = new { Id = roleToRemove.Id };
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("useroperationclaim/delete", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Rol başarıyla kaldırıldı");
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
                _logger.LogError(ex, "Rol kaldırılırken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateUserRolesAsync(int userId, List<int> roleIds)
        {
            try
            {
                SetAuthHeader();
                
                // Mevcut rolleri al
                var currentRoles = await GetUserRolesAsync(userId);
                var currentRoleIds = currentRoles.Select(r => r.OperationClaimId).ToList();

                // Kaldırılacak roller
                var rolesToRemove = currentRoleIds.Except(roleIds).ToList();
                foreach (var roleId in rolesToRemove)
                {
                    await RemoveRoleAsync(userId, roleId);
                }

                // Eklenecek roller
                var rolesToAdd = roleIds.Except(currentRoleIds).ToList();
                foreach (var roleId in rolesToAdd)
                {
                    await AddRoleAsync(userId, roleId);
                }

                return (true, "Roller başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Roller güncellenirken hata oluştu");
                return (false, "Bağlantı hatası oluştu");
            }
        }

        #endregion
    }

    // API Response model
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ApiDataResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
