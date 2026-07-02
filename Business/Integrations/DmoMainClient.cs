using Entities.Concrete.Integration;
using Core.Communication;
using Business.Abstract.IntegrationServices;

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
        var result = await _serviceCommunication.GetAsync<DmoUserResponse>("gateway", $"main-db/{username}/get-dmo-user-by-username");
        return result;
    }
    public async Task<List<ProxyEbysUser?>> GetDmoProxyUsersUserByEbysId(string ebysId)
    {
        var result = await _serviceCommunication.GetAsync<List<ProxyEbysUser>>("gateway", $"main-db/{ebysId}/get-dmo-proxy-users-by-ebysid");
        return result;
    }
}
