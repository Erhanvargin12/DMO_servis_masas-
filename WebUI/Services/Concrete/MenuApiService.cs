using Newtonsoft.Json;
using System.Text;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete;

public class MenuApiService : IMenuApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MenuApiService> _logger;

    public MenuApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<MenuApiService> logger)
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

    public async Task<List<MenuViewModel>> GetMenusByUserIdAsync(int userId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"menu/getbyuserid/{userId}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiDataResponse<List<MenuViewModel>>>(content);
                return result?.Data ?? new List<MenuViewModel>();
            }

            _logger.LogWarning("Menü getirme başarısız: {StatusCode}", response.StatusCode);
            return new List<MenuViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menü getirirken hata oluştu");
            return new List<MenuViewModel>();
        }
    }

    public async Task<List<MenuViewModel>> GetAllAsync()
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync("menu/getall");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiDataResponse<List<MenuViewModel>>>(content);
                return result?.Data ?? new List<MenuViewModel>();
            }

            return new List<MenuViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menüler getirilirken hata oluştu");
            return new List<MenuViewModel>();
        }
    }

    public async Task<MenuViewModel?> GetByIdAsync(int id)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"menu/getbyid/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiDataResponse<MenuViewModel>>(content);
                return result?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menü getirilirken hata oluştu");
            return null;
        }
    }

    public async Task<(bool Success, string? Message)> AddAsync(MenuViewModel menu)
    {
        try
        {
            SetAuthHeader();

            var addDto = new
            {
                Id = 0,
                Name = menu.Name,
                Description = menu.Description,
                Icon = menu.Icon,
                Url = menu.Url,
                ParentId = menu.ParentId,
                DisplayOrder = menu.DisplayOrder,
                IsActive = menu.IsActive
            };

            var json = JsonConvert.SerializeObject(addDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("menu/add", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Menü başarıyla eklendi");
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
            _logger.LogError(ex, "Menü eklenirken hata oluştu");
            return (false, "Bağlantı hatası oluştu");
        }
    }

    public async Task<(bool Success, string? Message)> UpdateAsync(MenuViewModel menu)
    {
        try
        {
            SetAuthHeader();

            var updateDto = new
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                Icon = menu.Icon,
                Url = menu.Url,
                ParentId = menu.ParentId,
                DisplayOrder = menu.DisplayOrder,
                IsActive = menu.IsActive
            };

            var json = JsonConvert.SerializeObject(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("menu/update", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Menü başarıyla güncellendi");
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
            _logger.LogError(ex, "Menü güncellenirken hata oluştu");
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
            var response = await _httpClient.PostAsync("menu/delete", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Menü başarıyla silindi");
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
            _logger.LogError(ex, "Menü silinirken hata oluştu");
            return (false, "Bağlantı hatası oluştu");
        }
    }

    #region Rol İşlemleri

    public async Task<List<int>> GetMenuRolesAsync(int menuId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"menu/getclaims/{menuId}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiDataResponse<List<MenuOperationClaimDto>>>(content);
                return result?.Data?.Select(x => x.OperationClaimId).ToList() ?? new List<int>();
            }

            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menü rolleri getirilirken hata oluştu");
            return new List<int>();
        }
    }

    public async Task<(bool Success, string? Message)> UpdateMenuRolesAsync(int menuId, List<int> roleIds)
    {
        try
        {
            SetAuthHeader();

            // Mevcut rolleri al
            var currentRoles = await GetMenuRolesAsync(menuId);

            // Kaldırılacak roller
            var rolesToRemove = currentRoles.Except(roleIds).ToList();
            foreach (var roleId in rolesToRemove)
            {
                await RemoveMenuRoleAsync(menuId, roleId);
            }

            // Eklenecek roller
            var rolesToAdd = roleIds.Except(currentRoles).ToList();
            foreach (var roleId in rolesToAdd)
            {
                await AddMenuRoleAsync(menuId, roleId);
            }

            return (true, "Menü rolleri başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menü rolleri güncellenirken hata oluştu");
            return (false, "Bağlantı hatası oluştu");
        }
    }

    private async Task AddMenuRoleAsync(int menuId, int roleId)
    {
        var dto = new { MenuId = menuId, OperationClaimId = roleId };
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("menu/addclaim", content);
    }

    private async Task RemoveMenuRoleAsync(int menuId, int roleId)
    {
        // Önce claim'i bul
        var response = await _httpClient.GetAsync($"menu/getclaims/{menuId}");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var result = JsonConvert.DeserializeObject<ApiDataResponse<List<MenuOperationClaimDto>>>(responseContent);
            var claim = result?.Data?.FirstOrDefault(x => x.OperationClaimId == roleId);
            
            if (claim != null)
            {
                var dto = new { Id = claim.Id, MenuId = menuId, OperationClaimId = roleId };
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync("menu/deleteclaim", content);
            }
        }
    }

    #endregion

    private class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    private class ApiDataResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    private class MenuOperationClaimDto
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int OperationClaimId { get; set; }
    }
}
