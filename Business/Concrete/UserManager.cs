using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Aspects.Caching;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.Win32;
using System.Linq.Expressions;

namespace Business.Concrete;

public class UserManager : IUserService
{
    private IUserDal _userDal;
    public UserManager(IUserDal userDal)
    {
        _userDal = userDal;
    }
    public async Task<IResult> AddAsync(User user)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfUserMailExistsAsync(user.Mail),
            () => CheckIfUserRegistrationIdExistsAsync(user.RegistirationId));
        
        if (result != null)
        {
            return result;
        }
        await _userDal.AddAsync(user);
        return new SuccessResult(Messages.UserAdded);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> UpdateAsync(User user)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfUserExistsAsync(user.Id),
            () => CheckIfUserMailExistsForUpdateAsync(user.Id, user.Mail),
            () => CheckIfUserRegistrationIdExistsForUpdateAsync(user.Id, user.RegistirationId)
        );

        if (result != null)
        {
            return result;
        }

        var existingUser = await _userDal.GetAsync(u => u.Id == user.Id);
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Mail = user.Mail;
        existingUser.Status = user.Status;
        existingUser.RegistirationId = user.RegistirationId;
        existingUser.OrganizationUnitId = user.OrganizationUnitId;
        existingUser.PositionId = user.PositionId;
        existingUser.UpdateAt = DateTime.Now;

        await _userDal.UpdateAsync(existingUser);
        return new SuccessResult(Messages.UserUpdated);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> DeleteAsync(User user)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfUserExistsAsync(user.Id));

        if (result != null)
        {
            return result;
        }

        var existingUser = await _userDal.GetAsync(u => u.Id == user.Id);
        await _userDal.DeleteAsync(existingUser);
        return new SuccessResult(Messages.UserDeleted);
    }
    public async Task<IDataResult<List<User>>> GetAllAsync(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<List<User>>(await _userDal.GetAllAsync(filter), Messages.UsersList);
    }
    public async Task<IDataResult<User>> GetUserAsync(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<User>(await _userDal.GetAsync(filter), Messages.UsersList);
    }
    public async Task<List<OperationClaim>> GetClaimsAsync(User user)
    {
        return await _userDal.GetClaimsAsync(user);
    }



    #region Business Rules
    private async Task<IResult> CheckIfUserMailExistsAsync(string email)
    {
        var result = await _userDal.GetAllAsync(e => e.Mail == email);
        if (result is not null && result.Count > 0)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserMailExistsForUpdateAsync(int id, string email)
    {
        var result = await _userDal.GetAllAsync(e => e.Mail == email && e.Id != id);
        if (result is not null && result.Count > 0)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserRegistrationIdExistsAsync(string registrationId)
    {
        if (string.IsNullOrWhiteSpace(registrationId)) return new SuccessResult();

        var result = await _userDal.GetAllAsync(e => e.RegistirationId == registrationId);
        if (result is not null && result.Count > 0)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserRegistrationIdExistsForUpdateAsync(int id, string registrationId)
    {
        if (string.IsNullOrWhiteSpace(registrationId)) return new SuccessResult();

        var result = await _userDal.GetAllAsync(e => e.RegistirationId == registrationId && e.Id != id);
        if (result is not null && result.Count > 0)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserExistsAsync(int id)
    {
        var result = await _userDal.GetAsync(u => u.Id == id);
        if (result == null)
        {
            return new ErrorResult(Messages.UserNotFound);
        }
        return new SuccessResult();
    }
    #endregion
}
