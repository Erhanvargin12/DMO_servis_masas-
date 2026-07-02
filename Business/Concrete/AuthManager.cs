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
using Entities.Concrete.Integration;
using Entities.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenHelper _tokenHelper;
    private readonly IRefreshTokenDal _refreshTokenDal;
    private readonly IDmoMainService _dmoMainService;
    private readonly IBranchService _branchService;
    private readonly IOperationClaimService _operationClaimService;
    private readonly IUserOperationClaimService _userOperationClaimService;

    public AuthManager(
        IUserService userService,
        ITokenHelper tokenHelper,
        IRefreshTokenDal refreshTokenDal,
        IDmoMainService dmoMainService,
        IBranchService branchService,
        IOperationClaimService operationClaimService,
        IUserOperationClaimService userOperationClaimService)
    {
        _userService = userService;
        _tokenHelper = tokenHelper;
        _refreshTokenDal = refreshTokenDal;
        _dmoMainService = dmoMainService;
        _branchService = branchService;
        _operationClaimService = operationClaimService;
        _userOperationClaimService = userOperationClaimService;
    }

    private async Task<IDataResult<User>> AddStaffUserAsync(DmoUserResponse staff)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfUserMailExistsAsync(staff.Email));

        if (result != null)
        {
            return new ErrorDataResult<User>(result.Message);
        }

        await _branchService.AddIfNotExistsAsync(new Branch
        {
            DepartmentId = staff.DepartmentId,
            ParentDepartmentId = staff.ParentDepartmentId,
            Name = staff.Department?.Split('\\').LastOrDefault()?.Trim() ?? string.Empty
        });

        if (!string.IsNullOrWhiteSpace(staff.ParentDepartmentId))
        {
            var parentName = staff.Department?.Split('\\').ElementAtOrDefault(1)?.Trim() ?? string.Empty;
            await _branchService.AddIfNotExistsAsync(new Branch
            {
                DepartmentId = staff.ParentDepartmentId,
                ParentDepartmentId = null,
                Name = parentName
            });
        }

        var user = new User
        {
            Mail = staff.Email,
            FirstName = staff.Name,
            LastName = staff.Surname,
            BranchId = staff.DepartmentId,
            PositionId = (int)staff.PositionType,
            EbysUserId = staff.EbysId,
            CreateAt = DateTime.Now,
            Status = true,
        };

        var addResult = await _userService.AddAsync(user);
        if (!addResult.Success)
        {
            return new ErrorDataResult<User>(addResult.Message);
        }

        await AssignRoleIfExistsAsync(user.Id, "Tickets");
        
        bool isAdmin = staff.Roles.Any(r =>r.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) || r.Equals("Admin", StringComparison.OrdinalIgnoreCase));
        if (isAdmin)
        {
            await AssignRoleIfExistsAsync(user.Id, "Admin");
        }

        return new SuccessDataResult<User>(user, Messages.UserAdded);
    }

    private async Task AssignRoleIfExistsAsync(int userId, string roleName)
    {
        var allClaims = await _operationClaimService.GetAllAsync(c => c.Name == roleName);
        var claim = allClaims.Data?.FirstOrDefault();
        if (claim == null) return;

        await _userOperationClaimService.AddAsync(new UserOperationClaim
        {
            UserId = userId,
            OperationClaimId = claim.Id
        });
    }

    public async Task<IDataResult<User>> LoginAsync(UserForLoginDto userForLoginDto)
    {
        var userResult = await _userService.GetUserAsync(u=>u.Mail == userForLoginDto.Email);
        var userToCheck = userResult.Data;
        
        if (userToCheck is null)
        {
            var user = await _dmoMainService.GetDmoUserByUserName(userForLoginDto.Email);
            var proxyUser = await _dmoMainService.GetDmoProxyUsersUserByEbysId(user.EbysId);
            if (user is null)
                return new ErrorDataResult<User>(Messages.UserNotFound);

            var registerResult = await AddStaffUserAsync(user);

            if (!registerResult.Success)
            {
                return new ErrorDataResult<User>(registerResult.Message);
            }

            userToCheck = registerResult.Data;
        }

        if (!userToCheck.Status)
        {
            return new ErrorDataResult<User>(Messages.UserNotActive);
        }

        //if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.PasswordHash, userToCheck.PasswordSalt))
        //{
        //    return new ErrorDataResult<User>(Messages.PasswordError);
        //}

        return new SuccessDataResult<User>(userToCheck, Messages.SuccessfulLogin);
    }
    public async Task<IDataResult<AccessToken>> CreateAccessTokenAsync(User user, string? ipAddress = null)
    { 
        var proxyEbysUser = await _dmoMainService.GetDmoProxyUsersUserByEbysId(user.EbysUserId);
        var ebysIds = proxyEbysUser.Select(x => x.EbysId).ToList();
        if (proxyEbysUser != null)
        {
            var proxyUsers = await _userService.GetAllAsync(u => ebysIds.Contains(u.EbysUserId));
            user.ProxyUserId.AddRange(proxyUsers.Data.Select(u => u.Id.ToString()));
        }

        var claims = await _userService.GetClaimsAsync(user);
        var accessToken = _tokenHelper.CreateToken(user, claims);
        await SaveRefreshTokenAsync(user.Id, accessToken.RefreshToken, accessToken.RefreshTokenExpiration, ipAddress);
        
        return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
    }

    public async Task<IDataResult<AccessToken>> RefreshTokenAsync(string token, string refreshToken, string? ipAddress)
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
        var hashedToken = HashingHelper.ComputeSha256Hash(refreshToken);
        var storedToken = await _refreshTokenDal.GetAsync(rt => 
            rt.UserId == userId && 
            rt.Token == hashedToken);

        if (storedToken == null)
        {
            return new ErrorDataResult<AccessToken>(Messages.InvalidRefreshToken);
        }

        // Token iptal edilmiş mi kontrol et
        if (storedToken.IsRevoked)
        {
            // Token reuse attack tespit edildi - tüm user token'larını iptal et
            await RevokeAllUserTokensAsync(userId, ipAddress);
            return new ErrorDataResult<AccessToken>(Messages.TokenReuseDetected);
        }

        // Token süresi dolmuş mu?
        if (storedToken.IsExpired)
        {
            return new ErrorDataResult<AccessToken>(Messages.RefreshTokenExpired);
        }

        // Kullanıcıyı getir
        var userResult = await _userService.GetAllAsync(u => u.Id == userId);
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
        var claims = await _userService.GetClaimsAsync(user);
        var newAccessToken = _tokenHelper.CreateToken(user, claims);
        
        // Eski token'a yeni token referansı ekle (Hashed)
        var newHashedToken = HashingHelper.ComputeSha256Hash(newAccessToken.RefreshToken);
        storedToken.ReplacedByToken = newHashedToken;
        await _refreshTokenDal.UpdateAsync(storedToken);
        
        // Yeni refresh token'ı kaydet
        await SaveRefreshTokenAsync(user.Id, newAccessToken.RefreshToken, newAccessToken.RefreshTokenExpiration, ipAddress);

        return new SuccessDataResult<AccessToken>(newAccessToken, Messages.TokenRefreshed);
    }

    public async Task<IResult> RevokeTokenAsync(string refreshToken, string? ipAddress)
    {
        var hashedToken = HashingHelper.ComputeSha256Hash(refreshToken);
        var storedToken = await _refreshTokenDal.GetAsync(rt => rt.Token == hashedToken);
        
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
        await _refreshTokenDal.UpdateAsync(storedToken);

        return new SuccessResult(Messages.TokenRevoked);
    }

    public async Task<IResult> RevokeAllUserTokensAsync(int userId, string? ipAddress)
    {
        var userTokens = await _refreshTokenDal.GetAllAsync(rt => rt.UserId == userId && rt.Revoked == null);
        
        foreach (var token in userTokens)
        {
            token.Revoked = DateTime.Now;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Security: All tokens revoked";
            await _refreshTokenDal.UpdateAsync(token);
        }

        return new SuccessResult(Messages.AllTokensRevoked);
    }

    private async Task SaveRefreshTokenAsync(int userId, string token, DateTime expires, string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = HashingHelper.ComputeSha256Hash(token),
            Expires = expires,
            Created = DateTime.Now,
            CreatedByIp = ipAddress
        };
        await _refreshTokenDal.AddAsync(refreshToken);
    }

    
    #region Business Rules
    private async Task<IResult> CheckIfUserMailExistsAsync(string email)
    {
        var userResult = await _userService.GetAllAsync(m => m.Mail == email);
        if (userResult.Data?.FirstOrDefault() != null)
        {
            return new ErrorResult(Messages.UserAlreadyExists);
        }
        return new SuccessResult();
    }
    #endregion
}
