using System.Security.Claims;
using Core.Entities.Concrete;
using Entities.Concrete;

namespace Core.Utilities.Security.Jwt
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user, List<OperationClaim> operationClaims);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
