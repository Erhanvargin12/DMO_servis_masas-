using Entities.Concrete.Integration;
using Entities.DTOs;

namespace Business.Abstract.IntegrationServices;

public interface IDmoAuthService
{
    Task<DmoIntegrationLoginResponse?> DmoLoginAsync(UserForLoginDto loginDto);
}
