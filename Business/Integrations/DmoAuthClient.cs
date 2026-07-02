using Entities.Concrete.Integration;
using Core.Communication;
using Entities.DTOs;
using Business.Abstract.IntegrationServices;

namespace Business.Integrations;

public class DmoAuthClient : IDmoAuthService
{
    private readonly IServiceCommunication _service;

    public DmoAuthClient(IServiceCommunication service)
    {
        _service = service;
    }

    public async Task<DmoIntegrationLoginResponse?> DmoLoginAsync(UserForLoginDto loginDto)
    {
        var result = await _service.PostAsync<UserForLoginDto, DmoIntegrationLoginResponse>(
            "gateway", "dmo-login", loginDto);

        return result;
    }
}
