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

    //[CacheRemoveAspect("IUserService.Get")]
    //[SecuredOperation("user, post, admin")]
    //[ValidationAspect(typeof(UserValidator))]
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

    public IResult Delete(User user)
    {

        _userDal.Delete(user);
        return new SuccessResult(Messages.UserDeleted);
    }

    //[CacheAspect]
    public IDataResult<List<User>> GetAll(Expression<Func<User, bool>> filter = null)
    {
        return new SuccessDataResult<List<User>>(_userDal.GetAll(filter),Messages.UsersList);
    }


    public List<OperationClaim> GetClaims(User user)
    {
        return _userDal.GetClaims(user);
    }


    public IResult Update(User user)
    {
        _userDal.Update(user);
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
    private IResult CheckIfUserRegistrationIdExists(int registrationId)
    {
        var result = _userDal.GetAll(e => e.RegistirationId == registrationId).Any();
        if (result)
        {
            return new ErrorResult(Messages.UserRegistrationIdAlreadyExists);
        }
        return new SuccessResult();
    }
    #endregion
}
