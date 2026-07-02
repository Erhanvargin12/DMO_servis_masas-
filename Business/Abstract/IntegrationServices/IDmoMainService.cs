using Entities.Concrete.Integration;

namespace Business.Abstract.IntegrationServices;

public interface IDmoMainService
{
    Task<DmoUserResponse?> GetDmoUserByUserName(string username);
    Task<List<ProxyEbysUser?>> GetDmoProxyUsersUserByEbysId(string ebysId);
}
