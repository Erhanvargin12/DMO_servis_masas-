using Core.Utilities.Results;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Services.Abstract;

namespace WebUI.Core.Services.Concrete
{
    public class BaseController : Controller
    {
        private readonly IApiClient _apiClient;

        public BaseController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        protected async Task<IDataResult<T>> SendRequest<T>(string endpoint, object payload = null, HttpMethod method = null)
        {
            method ??= HttpMethod.Get;
            SetUserToken();

            IDataResult<T> result;

            switch (method.Method.ToUpper())
            {
                case "POST":
                    result = await _apiClient.PostAsync<T>(endpoint, payload);
                    break;
                case "PUT":
                    result = await _apiClient.PutAsync<T>(endpoint, payload);
                    break;
                case "DELETE":
                    result = await _apiClient.DeleteAsync<T>(endpoint);
                    break;
                default:
                    result = await _apiClient.GetAsync<T>(endpoint);
                    break;
            }

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            return result;
        }

        private void SetUserToken()
        {
            var token = HttpContext.Session.GetString("AccessToken") 
                     ?? User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token)) 
                _apiClient.SetBearerToken(token);
        }
    }
}
