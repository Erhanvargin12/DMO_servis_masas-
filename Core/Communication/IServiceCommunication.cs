namespace Core.Communication;

public interface IServiceCommunication
{
    Task<TResponse?> GetAsync<TResponse>(string serviceId, string address);
    Task<TResponse?> GetAsync<T, TResponse>(string serviceId, string address, T queryData);
    Task<TResponse?> PostAsync<T, TResponse>(string serviceId, string address, T data);
    Task PostAsync<T>(string serviceId, string address, T data);
    Task<Stream> PostStreamAsync<T>(string serviceId, string address, T data);
    Task PostFileUploadAsync(string serviceId, string address, Stream fileStream, string fileName, string fileContentName, string contentType, Dictionary<string, HttpContent> contents);
    Task PutAsync<T>(string serviceId, string address, T data);
    Task<TResponse?> PutAsync<T, TResponse>(string serviceId, string address, T data);
    Task<TResponse?> PatchAsync<T, TResponse>(string serviceId, string address, T data);
    Task DeleteAsync(string serviceId, string address);
}
