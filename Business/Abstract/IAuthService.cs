using Core.Utilities.Results;
using Core.Utilities.Security.Jwt;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IAuthService
    {
        Task<IDataResult<User>> Login(UserForLoginDto userForLoginDto);
        Task<IDataResult<User>> Register(UserForRegisterDto userForRegisterDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(User user, string? ipAddress = null);
        IDataResult<AccessToken> RefreshToken(string token, string refreshToken, string? ipAddress);
        IResult RevokeToken(string refreshToken, string? ipAddress);
        IResult RevokeAllUserTokens(int userId, string? ipAddress);
    }
}
