using Business.Abstract.IntegrationServices;

namespace Business.Integrations;

public class DmoEbysClient : IDmoEbysService
{
    private readonly HttpClient _httpClient;

    public DmoEbysClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}