using Business.Abstract.IntegrationServices;
using Core.Communication;
using Entities.Concrete.Integration;
using System.Net.Http.Json;

namespace Business.Integrations;

public class DmoMainClient : IDmoMainService
{
    private readonly IServiceCommunication _serviceCommunication;

    public DmoMainClient(IServiceCommunication serviceCommunication)
    {
        _serviceCommunication = serviceCommunication;
    }

    public async Task<DmoUserResponse?> GetDmoUserByUserName(string username)
    {
        var response = await _serviceCommunication.GetAsync<DmoUserResponse>("gateway",$"main-db/{username}/get-dmo-user-by-username");
        return response;
    }
}
