using Core.Utilities.Results;

namespace WebUI.Core.Services.Abstract;

public interface IApiClient
{
    Task<IDataResult<T>> GetAsync<T>(string endpoint);
    Task<IDataResult<T>> PostAsync<T>(string endpoint, object payload);
    Task<IDataResult<T>> PutAsync<T>(string endpoint, object payload);
    Task<IDataResult<T>> DeleteAsync<T>(string endpoint);
    void SetBearerToken(string token);
}
