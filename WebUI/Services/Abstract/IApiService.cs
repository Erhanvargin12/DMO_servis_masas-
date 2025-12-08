namespace WebUI.Services.Abstract
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T, TRequest>(string endpoint, TRequest data);
        Task<T?> PutAsync<T, TRequest>(string endpoint, TRequest data);
        Task<bool> DeleteAsync(string endpoint);
        void SetBearerToken(string token);
        void ClearBearerToken();
    }
}

