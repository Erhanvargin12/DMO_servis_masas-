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
using System.Linq.Expressions;

namespace Business.Concrete;

public class UserManager : IUserService
{
    private IUserDal _userDal;
    public UserManager(IUserDal userDal)
    {
        _userDal = userDal;
    }

    // Auth işlemleri için SecuredOperation YOK - Login/Register için gerekli
    public IResult Add(User user)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserMailExists(user.Mail),
            CheckIfUserRegistrationIdExists(user.RegistirationId)
        );
        
        if (result != null)
        {
            return result;
        }
        _userDal.Add(user);
        return new SuccessResult(Messages.UserAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Delete(User user)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserExists(user.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingUser = _userDal.Get(u => u.Id == user.Id);
        _userDal.Delete(existingUser);
        return new SuccessResult(Messages.UserDeleted);
    }

    // Auth işlemleri için SecuredOperation YOK - Login için gerekli
    public IDataResult<List<User>> GetAll(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<List<User>>(_userDal.GetAll(filter), Messages.UsersList);
    }

    // Token oluşturmak için SecuredOperation YOK
    public List<OperationClaim> GetClaims(User user)
    {
        return _userDal.GetClaims(user);
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

    #region Business Rules
    private IResult CheckIfUserMailExists(string email)
    {
        var result = _userDal.GetAll(e => e.Mail == email).Any();
        if (result)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserMailExistsForUpdate(int id, string email)
    {
        var result = _userDal.GetAll(e => e.Mail == email && e.Id != id).Any();
        if (result)
        {
            return new ErrorResult(Messages.UserMailAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserRegistrationIdExists(int registrationId)
    {
        var result = _userDal.GetAll(e => e.RegistirationId == registrationId).Any();
        if (result)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserRegistrationIdExistsForUpdate(int id, int registrationId)
    {
        var result = _userDal.GetAll(e => e.RegistirationId == registrationId && e.Id != id).Any();
        if (result)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserExists(int id)
    {
        var result = _userDal.Get(u => u.Id == id);
        if (result == null)
        {
            return new ErrorResult(Messages.UserNotFound);
        }
        return new SuccessResult();
    }
    #endregion
}
