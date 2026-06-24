using Business.Abstract.IntegrationServices;
using Entities.Concrete.Integration;
using Entities.DTOs;
using System.Net.Http.Json;

namespace Business.Integrations;

public class DmoAuthClient : IDmoAuthService
{
    private readonly HttpClient _httpClient;

    public DmoAuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<DmoIntegrationLoginResponse?> DmoLoginAsync(UserForLoginDto loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync("dmo-login",loginDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DmoIntegrationLoginResponse>();
    }
}
