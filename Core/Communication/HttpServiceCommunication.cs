using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

using Microsoft.AspNetCore.Http;

namespace Core.Communication;

public class HttpServiceCommunication : IServiceCommunication
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HttpServiceCommunication(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory), "HttpClientFactory not registered");
        _httpContextAccessor = httpContextAccessor;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private HttpClient CreateClientWithAuth(string serviceId)
    {
        var client = _httpClientFactory.CreateClient(serviceId);
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            string? token = null;
            if (httpContext.Session != null && httpContext.Session.TryGetValue("AccessToken", out var bytes) && bytes != null)
            {
                token = System.Text.Encoding.UTF8.GetString(bytes);
            }
            if (string.IsNullOrEmpty(token))
            {
                token = httpContext.User?.FindFirst("AccessToken")?.Value;
            }

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        return client;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string serviceId, string address)
    {
        var client = CreateClientWithAuth(serviceId);

        var httpResponse = await client.GetAsync(address);
        if (!httpResponse.IsSuccessStatusCode)
        {
            return default;
        }

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseContent))
            return default;

        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonSerializerOptions);
    }

    public Task<TResponse?> GetAsync<TResponse>(string serviceId,string address,object? queryData = null)
    {
        if (queryData is null)
            return GetAsync<TResponse>(serviceId, address);

        var props = queryData.GetType().GetProperties();

        var query = string.Join("&", props.Where(p => p.GetValue(queryData) != null)
            .Select(p =>
            {
                var value = p.GetValue(queryData)!;
                return $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(value.ToString()!)}";
            }));

        if (!string.IsNullOrWhiteSpace(query))
            address = $"{address}?{query}";

        return GetAsync<TResponse>(serviceId, address);
    }

    public async Task<TResponse?> PostAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        var responseMessage = await client.PostAsJsonAsync(address, data, _jsonSerializerOptions);

        var responseData = await responseMessage.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseData))
            return default;

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task PostAsync<T>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        await client.PostAsJsonAsync(address, data, _jsonSerializerOptions);
    }

    public async Task<Stream> PostStreamAsync<T>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        var responseMessage = await client.PostAsJsonAsync(address, data, _jsonSerializerOptions);

        return await responseMessage.Content.ReadAsStreamAsync();
    }

    public async Task PostFileUploadAsync(string serviceId, string address, Stream fileStream, string fileName,
        string fileContentName, string contentType, Dictionary<string, HttpContent> contents)
    {
        var client = CreateClientWithAuth(serviceId);

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        content.Add(fileContent, fileContentName, fileName);

        foreach (var contentItem in contents)
        {
            content.Add(contentItem.Value, contentItem.Key);
        }

        await client.PostAsync(address, content);
    }

    public async Task<TResponse?> PutAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        var responseMessage = await client.PutAsJsonAsync(address, data, _jsonSerializerOptions);

        var responseData = await responseMessage.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseData))
            return default;

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task PutAsync<T>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        await client.PutAsJsonAsync(address, data, _jsonSerializerOptions);
    }

    public async Task<TResponse?> PatchAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = CreateClientWithAuth(serviceId);

        var responseMessage = await client.PatchAsJsonAsync(address, data, _jsonSerializerOptions);

        var responseData = await responseMessage.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseData))
            return default;

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task DeleteAsync(string serviceId, string address)
    {
        var client = CreateClientWithAuth(serviceId);

        await client.DeleteAsync(address);
    }
}
