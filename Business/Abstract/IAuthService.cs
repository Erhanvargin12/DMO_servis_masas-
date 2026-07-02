using Core.Utilities.Results;
using Core.Utilities.Security.Jwt;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IAuthService
    {
        Task<IDataResult<User>> LoginAsync(UserForLoginDto userForLoginDto);
        Task<IDataResult<AccessToken>> CreateAccessTokenAsync(User user, string? ipAddress = null);
        Task<IDataResult<AccessToken>> RefreshTokenAsync(string token, string refreshToken, string? ipAddress);
        Task<IResult> RevokeTokenAsync(string refreshToken, string? ipAddress);
        Task<IResult> RevokeAllUserTokensAsync(int userId, string? ipAddress);
    }
}
