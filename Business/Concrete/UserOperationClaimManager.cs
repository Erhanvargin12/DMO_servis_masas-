using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Concrete;

public class UserOperationClaimManager : IUserOperationClaimService
{
    private readonly IUserOperationClaimDal _userOperationClaimDal;
    private readonly IUserDal _userDal;
    private readonly IOperationClaimDal _operationClaimDal;

    public UserOperationClaimManager(
        IUserOperationClaimDal userOperationClaimDal,
        IUserDal userDal,
        IOperationClaimDal operationClaimDal)
    {
        _userOperationClaimDal = userOperationClaimDal;
        _userDal = userDal;
        _operationClaimDal = operationClaimDal;
    }

    [SecuredOperation("Admin")]
    public IDataResult<List<UserOperationClaim>> GetAll(Expression<Func<UserOperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<UserOperationClaim>>(
            _userOperationClaimDal.GetAll(filter), 
            Messages.UserOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public IDataResult<List<UserOperationClaim>> GetByUserId(int userId)
    {
        return new SuccessDataResult<List<UserOperationClaim>>(
            _userOperationClaimDal.GetAll(u => u.UserId == userId),
            Messages.UserOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public IResult Add(UserOperationClaim userOperationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserExists(userOperationClaim.UserId),
            CheckIfOperationClaimExists(userOperationClaim.OperationClaimId),
            CheckIfUserOperationClaimAlreadyExists(userOperationClaim.UserId, userOperationClaim.OperationClaimId)
        );

        if (result != null)
        {
            return result;
        }

        _userOperationClaimDal.Add(userOperationClaim);
        return new SuccessResult(Messages.UserOperationClaimAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Delete(UserOperationClaim userOperationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserOperationClaimExists(userOperationClaim.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = _userOperationClaimDal.Get(u => u.Id == userOperationClaim.Id);
        _userOperationClaimDal.Delete(existingClaim);
        return new SuccessResult(Messages.UserOperationClaimDeleted);
    }

    [SecuredOperation("Admin")]
    public IResult DeleteByUserId(int userId)
    {
        IResult result = BusinessRules.Run(
            CheckIfUserExists(userId)
        );

        if (result != null)
        {
            return result;
        }

        var claims = _userOperationClaimDal.GetAll(u => u.UserId == userId);
        foreach (var claim in claims)
        {
            _userOperationClaimDal.Delete(claim);
        }
        return new SuccessResult(Messages.UserOperationClaimsDeleted);
    }

    #region Business Rules

    private IResult CheckIfUserExists(int userId)
    {
        var user = _userDal.Get(u => u.Id == userId);
        if (user == null)
        {
            return new ErrorResult(Messages.UserNotFound);
        }
        return new SuccessResult();
    }

    private IResult CheckIfOperationClaimExists(int operationClaimId)
    {
        var claim = _operationClaimDal.Get(o => o.Id == operationClaimId);
        if (claim == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserOperationClaimAlreadyExists(int userId, int operationClaimId)
    {
        var exists = _userOperationClaimDal.GetAll(u => 
            u.UserId == userId && u.OperationClaimId == operationClaimId).Any();
        
        if (exists)
        {
            return new ErrorResult(Messages.UserOperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfUserOperationClaimExists(int id)
    {
        var claim = _userOperationClaimDal.Get(u => u.Id == id);
        if (claim == null)
        {
            return new ErrorResult(Messages.UserOperationClaimNotFound);
        }
        return new SuccessResult();
    }

    #endregion
}
