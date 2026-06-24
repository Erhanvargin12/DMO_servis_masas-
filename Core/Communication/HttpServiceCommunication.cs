using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Core.Communication;

public class HttpServiceCommunication : IServiceCommunication
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HttpServiceCommunication(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory),"HttpClientFactory not registered");

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<TResponse?> GetAsync<TResponse>(string serviceId, string address)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        string response = await client.GetStringAsync(address);
        if (string.IsNullOrEmpty(response))
            return default;

        return JsonSerializer.Deserialize<TResponse>(response, _jsonSerializerOptions);
    }

    public Task<TResponse?> GetAsync<T, TResponse>(string serviceId, string address, T queryData)
    {
        var props = typeof(T).GetProperties();

        string query = string.Join("&", props.Where(r => r.GetValue(queryData) != null)
            .Select(r =>
            {
                var value = r.PropertyType == typeof(DateTime) || r.PropertyType == typeof(DateTime?)
                    ? HttpUtility.UrlEncode(DateTime.Parse(r.GetValue(queryData).ToString()).ToString("yyyy-MM-dd HH:mm"))
                    : HttpUtility.UrlEncode(r.GetValue(queryData).ToString());

                return $"{HttpUtility.UrlEncode(r.Name)}={value}";
            }));
        string newAddress = $"{address}?{query}";
        return GetAsync<TResponse?>(serviceId, newAddress);
    }

    public async Task<TResponse?> PostAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        string json = JsonSerializer.Serialize(data);

        using var body = new StringContent(content: json, encoding: Encoding.UTF8, "application/json");
        var responseMessage = await client.PostAsJsonAsync<T>(address, data);

        string responseData = await responseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task<TResponse?> PutAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        //string json = JsonSerializer.Serialize(data);
        //using var body = new StringContent(content: json, encoding: Encoding.UTF8, "application/json");

        var responseMessage = await client.PutAsJsonAsync(address, data);

        string responseData = await responseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task<TResponse?> PatchAsync<T, TResponse>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        //string json = JsonSerializer.Serialize(data);
        //using var body = new StringContent(content: json, encoding: Encoding.UTF8, "application/json");

        var responseMessage = await client.PatchAsJsonAsync(address, data);

        string responseData = await responseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
    }

    public async Task DeleteAsync(string serviceId, string address)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        await client.DeleteAsync(address);
    }

    public async Task PostAsync<T>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        await client.PostAsJsonAsync(address, data);
    }

    public async Task<Stream> PostStreamAsync<T>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        var responseMessage = await client.PostAsJsonAsync(address, data);

        Stream responseStream = await responseMessage.Content.ReadAsStreamAsync();

        return responseStream;
    }

    public async Task PutAsync<T>(string serviceId, string address, T data)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

        await client.PutAsJsonAsync(address, data);
    }

    public async Task PostFileUploadAsync(string serviceId, string address, Stream fileStream, string fileName, string fileContentName, string contentType, Dictionary<string, HttpContent> contents)
    {
        var client = _httpClientFactory.CreateClient(serviceId);

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
}
