using Business.Abstract;
using Business.Abstract.IntegrationServices;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Business;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenHelper _tokenHelper;
    private readonly IRefreshTokenDal _refreshTokenDal;
    private readonly IDmoMainService _dmoMainService;

    public AuthManager(
        IUserService userService, 
        ITokenHelper tokenHelper,
        IRefreshTokenDal refreshTokenDal,
        IDmoMainService dmoMainService)
    {
        _userService = userService;
        _tokenHelper = tokenHelper;
        _refreshTokenDal = refreshTokenDal;
        _dmoMainService = dmoMainService;
    }

    public async Task<IDataResult<User>> Register(UserForRegisterDto userForRegisterDto)
    {
        IResult result = await BusinessRules.Run(CheckIfUserMailExists(userForRegisterDto.Mail));

        if (result != null)
        {
            return new ErrorDataResult<User>(result.Message);
        }

        byte[] passwordHash, passwordSalt;
        HashingHelper.CreatePasswordHash(userForRegisterDto.Password, out passwordHash, out passwordSalt);
        
        var user = new User
        {
            Mail = userForRegisterDto.Mail,
            FirstName = userForRegisterDto.FirstName,
            LastName = userForRegisterDto.LastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            OrganizationUnitId = userForRegisterDto.BranchId,
            PositionId = userForRegisterDto.PositionId,
            RegistirationId = userForRegisterDto.RegistirationId,
            CreateAt = DateTime.Now,
            Status = true
        };
        
        var addResult = await _userService.Add(user);
        if (!addResult.Success)
        {
            return new ErrorDataResult<User>(addResult.Message);
        }
        
        return new SuccessDataResult<User>(user, Messages.UserAdded);
    }

    public async Task<IDataResult<User>> Login(UserForLoginDto userForLoginDto)
    {
        var userResult = _userService.GetUser(u=>u.Mail == userForLoginDto.Email);
        var userToCheck = userResult.Data;
        
        if (userToCheck is null)
        {
            var user = await _dmoMainService.GetDmoUserByUserName(userForLoginDto.Email);
            if (user is null)
                return new ErrorDataResult<User>(Messages.UserNotFound);

            var registerResult = await Register(new UserForRegisterDto
            {
                Mail = user.Email,
                FirstName = user.Name,
                LastName = user.Name,
                Password = userForLoginDto.Password,
                BranchId = user.BranchId,
                PositionId = (int)user.PositionType,
                RegistirationId = user.EbysId
            });
        }

        if (!userToCheck.Status)
        {
            return new ErrorDataResult<User>(Messages.UserNotActive);
        }

        if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.PasswordHash, userToCheck.PasswordSalt))
        {
            return new ErrorDataResult<User>(Messages.PasswordError);
        }

        return new SuccessDataResult<User>(userToCheck, Messages.SuccessfulLogin);
    }

    public IResult UserExists(string email)
    {
        var userResult = _userService.GetAll(m => m.Mail == email);
        if (userResult.Data?.FirstOrDefault() != null)
        {
            return new ErrorResult(Messages.UserAlreadyExists);
        }
        return new SuccessResult();
    }


    public IDataResult<AccessToken> CreateAccessToken(User user, string? ipAddress = null)
    {
        var claims = _userService.GetClaims(user);
        var accessToken = _tokenHelper.CreateToken(user, claims);
        
        // Refresh token'ı veritabanına kaydet
        SaveRefreshToken(user.Id, accessToken.RefreshToken, accessToken.RefreshTokenExpiration, ipAddress);
        
        return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
    }

    public IDataResult<AccessToken> RefreshToken(string token, string refreshToken, string? ipAddress)
    {
        // Expired access token'dan kullanıcı bilgisini al
        var principal = _tokenHelper.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return new ErrorDataResult<AccessToken>(Messages.InvalidToken);
        }

        var userIdClaim = principal.Claims.FirstOrDefault(c => 
            c.Type == System.Security.Claims.ClaimTypes.NameIdentifier ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return new ErrorDataResult<AccessToken>(Messages.InvalidToken);
        }

        // Refresh token'ı doğrula
        var storedToken = _refreshTokenDal.Get(rt => 
            rt.UserId == userId && 
            rt.Token == refreshToken);

        if (storedToken == null)
        {
            return new ErrorDataResult<AccessToken>(Messages.InvalidRefreshToken);
        }

        // Token iptal edilmiş mi kontrol et
        if (storedToken.IsRevoked)
        {
            // Token reuse attack tespit edildi - tüm user token'larını iptal et
            RevokeAllUserTokens(userId, ipAddress);
            return new ErrorDataResult<AccessToken>(Messages.TokenReuseDetected);
        }

        // Token süresi dolmuş mu?
        if (storedToken.IsExpired)
        {
            return new ErrorDataResult<AccessToken>(Messages.RefreshTokenExpired);
        }

        // Kullanıcıyı getir
        var userResult = _userService.GetAll(u => u.Id == userId);
        var user = userResult.Data?.FirstOrDefault();
        
        if (user == null || !user.Status)
        {
            return new ErrorDataResult<AccessToken>(Messages.UserNotFound);
        }

        // Eski token'ı iptal et (Token Rotation)
        storedToken.Revoked = DateTime.Now;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReasonRevoked = "Replaced by new token";
        
        // Yeni token oluştur
        var claims = _userService.GetClaims(user);
        var newAccessToken = _tokenHelper.CreateToken(user, claims);
        
        // Eski token'a yeni token referansı ekle
        storedToken.ReplacedByToken = newAccessToken.RefreshToken;
        _refreshTokenDal.Update(storedToken);
        
        // Yeni refresh token'ı kaydet
        SaveRefreshToken(user.Id, newAccessToken.RefreshToken, newAccessToken.RefreshTokenExpiration, ipAddress);

        return new SuccessDataResult<AccessToken>(newAccessToken, Messages.TokenRefreshed);
    }

    public IResult RevokeToken(string refreshToken, string? ipAddress)
    {
        var storedToken = _refreshTokenDal.Get(rt => rt.Token == refreshToken);
        
        if (storedToken == null)
        {
            return new ErrorResult(Messages.InvalidRefreshToken);
        }

        if (storedToken.IsRevoked)
        {
            return new ErrorResult(Messages.TokenAlreadyRevoked);
        }

        storedToken.Revoked = DateTime.Now;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReasonRevoked = "Revoked by user";
        _refreshTokenDal.Update(storedToken);

        return new SuccessResult(Messages.TokenRevoked);
    }

    public IResult RevokeAllUserTokens(int userId, string? ipAddress)
    {
        var userTokens = _refreshTokenDal.GetAll(rt => rt.UserId == userId && rt.Revoked == null);
        
        foreach (var token in userTokens)
        {
            token.Revoked = DateTime.Now;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Security: All tokens revoked";
            _refreshTokenDal.Update(token);
        }

        return new SuccessResult(Messages.AllTokensRevoked);
    }

    private void SaveRefreshToken(int userId, string token, DateTime expires, string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            Expires = expires,
            Created = DateTime.Now,
            CreatedByIp = ipAddress
        };
        _refreshTokenDal.Add(refreshToken);
    }

    #region Business Rules

    private IResult CheckIfUserMailExists(string email)
    {
        var userResult = _userService.GetAll(m => m.Mail == email);
        if (userResult.Data?.FirstOrDefault() != null)
        {
            return new ErrorResult(Messages.UserAlreadyExists);
        }
        return new SuccessResult();
    }

    #endregion
}
