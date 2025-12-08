using Business.Abstract;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete;

public class AuthManager : IAuthService
{
    private IUserService _userService;
    private ITokenHelper _tokenHelper;

    public AuthManager(IUserService userService, ITokenHelper tokenHelper)
    {
        _userService = userService;
        _tokenHelper = tokenHelper;
    }

    public IDataResult<User> Register(UserForRegisterDto userForRegisterDto)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserMailExists(userForRegisterDto.Mail)
        );

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
        
        var addResult = _userService.Add(user);
        if (!addResult.Success)
        {
            return new ErrorDataResult<User>(addResult.Message);
        }
        
        return new SuccessDataResult<User>(user, Messages.UserAdded);
    }

    public IDataResult<User> Login(UserForLoginDto userForLoginDto)
    {
        var userResult = _userService.GetAll(m => m.Mail == userForLoginDto.Email);
        var userToCheck = userResult.Data?.FirstOrDefault();
        
        if (userToCheck == null)
        {
            return new ErrorDataResult<User>(Messages.UserNotFound);
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

    public IDataResult<AccessToken> CreateAccessToken(User user)
    {
        var claims = _userService.GetClaims(user);
        var accessToken = _tokenHelper.CreateToken(user, claims);
        return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
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
