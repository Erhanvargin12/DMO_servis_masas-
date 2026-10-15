using Core.Utilities.Results;
using Microsoft.AspNetCore.Http;
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
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

    public async Task<IDataResult<T>> SendFormAsync<T>(
        string endpoint,
        Dictionary<string, string> fields,
        IFormFileCollection? files = null,
        string fileFieldName = "Attachments")
    {
        using var form = new MultipartFormDataContent();

        foreach (var kvp in fields)
            form.Add(new StringContent(kvp.Value ?? string.Empty), kvp.Key);

        if (files != null)
        {
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");
                form.Add(fileContent, fileFieldName, file.FileName);
            }
        }

        var response = await _httpClient.PostAsync(endpoint, form);
        return await HandleResponse<T>(response);
    }

    private async Task<IDataResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var result = JsonConvert.DeserializeObject<JsonDataResult<T>>(content);
                if (result != null)
                {
                    if (!response.IsSuccessStatusCode)
                        return new ErrorDataResult<T>(result.Message ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

                    return new DataResult<T>(result.Data, result.Success, result.Message);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                return new ErrorDataResult<T>($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
            }

            return new ErrorDataResult<T>("Response parse edilemedi.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "API Response parse hatası. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            return new ErrorDataResult<T>(!response.IsSuccessStatusCode ? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}" : "Response parse edilemedi.");
        }
    }
}

public class JsonDataResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
