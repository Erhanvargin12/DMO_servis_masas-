using Core.Utilities.Results;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WebUI.Core.Services.Abstract;
using WebUI.Models.ViewModels;
using WebUI.Services.Concrete;

namespace WebUI.Core.Services.Concrete;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(apiSettings.Value.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(apiSettings.Value.Timeout);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IDataResult<T>> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<IDataResult<T>> PostAsync<T>(string endpoint, object payload)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        return await HandleResponse<T>(response);
    }

    public async Task<IDataResult<T>> PutAsync<T>(string endpoint, object payload)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, payload);
        return await HandleResponse<T>(response);
    }

    public async Task<IDataResult<T>> DeleteAsync<T>(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    private async Task<IDataResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var result = JsonConvert.DeserializeObject<JsonDataResult<T>>(content);

            if (result == null)
                return new ErrorDataResult<T>("Response parse edilemedi.");

            if (!response.IsSuccessStatusCode)
                return new ErrorDataResult<T>(result.Message ?? $"HTTP Error: {response.StatusCode}");

            return new DataResult<T>(result.Data, result.Success, result.Message);
        }
        catch
        {
            return new ErrorDataResult<T>("Response parse edilemedi.");
        }
    }
}

public class JsonDataResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}