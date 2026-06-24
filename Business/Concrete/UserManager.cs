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
    public async Task<IResult> Add(User user)
    {
        IResult result = await BusinessRules.Run(
            CheckIfUserMailExists(user.Mail),
            CheckIfUserRegistrationIdExists(user.RegistirationId));
        
        if (result != null)
        {
            return result;
        }
        _userDal.Add(user);
        return new SuccessResult(Messages.UserAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Update(User user)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserExists(user.Id),
            CheckIfUserMailExistsForUpdate(user.Id, user.Mail),
            CheckIfUserRegistrationIdExistsForUpdate(user.Id, user.RegistirationId)
        );

        if (result != null)
        {
            return result;
        }

        var existingUser = _userDal.Get(u => u.Id == user.Id);
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Mail = user.Mail;
        existingUser.Status = user.Status;
        existingUser.RegistirationId = user.RegistirationId;
        existingUser.OrganizationUnitId = user.OrganizationUnitId;
        existingUser.PositionId = user.PositionId;
        existingUser.UpdateAt = DateTime.Now;

        _userDal.Update(existingUser);
        return new SuccessResult(Messages.UserUpdated);
    }

    [SecuredOperation("Admin")]
    public IResult Delete(User user)
    {
        IResult result = await BusinessRules.Run(CheckIfUserExists(user.Id));

        if (result != null)
        {
            return result;
        }

        var existingUser = await _userDal.Get(u => u.Id == user.Id);
        _userDal.Delete(existingUser);
        return new SuccessResult(Messages.UserDeleted);
    }
    public IDataResult<List<User>> GetAll(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<List<User>>(_userDal.GetAll(filter), Messages.UsersList);
    }
    public async Task<IDataResult<User>> GetUser(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<User>(await _userDal.Get(filter), Messages.UsersList);
    }
    public List<OperationClaim> GetClaims(User user)
    {
        return _userDal.GetClaims(user);
    }



    #region Business Rules
    private async Task<IResult> CheckIfUserMailExists(string email)
    {
        var result = await _userDal.GetAll(e => e.Mail == email);
        if (result is not null)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserMailExistsForUpdate(int id, string email)
    {
        var result = await _userDal.GetAll(e => e.Mail == email && e.Id != id);
        if (result is not null)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserRegistrationIdExists(string registrationId)
    {
        var result = await _userDal.GetAll(e => e.RegistirationId == registrationId);
        if (result is not null)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserRegistrationIdExistsForUpdate(int id, string registrationId)
    {
        var result = _userDal.GetAll(e => e.RegistirationId == registrationId && e.Id != id);
        if (result is not null)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserExists(int id)
    {
        var result = await _userDal.Get(u => u.Id == id);
        if (result == null)
        {
            return new ErrorResult(Messages.UserNotFound);
        }
        return new SuccessResult();
    }
    #endregion
}
