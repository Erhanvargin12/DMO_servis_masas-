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
    public async Task<IDataResult<List<UserOperationClaim>>> GetAllAsync(Expression<Func<UserOperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<UserOperationClaim>>(
            await _userOperationClaimDal.GetAllAsync(filter), 
            Messages.UserOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public async Task<IDataResult<List<UserOperationClaim>>> GetByUserIdAsync(int userId)
    {
        return new SuccessDataResult<List<UserOperationClaim>>(
            await _userOperationClaimDal.GetAllAsync(u => u.UserId == userId),
            Messages.UserOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> AddAsync(UserOperationClaim userOperationClaim)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfUserExistsAsync(userOperationClaim.UserId),
            () => CheckIfOperationClaimExistsAsync(userOperationClaim.OperationClaimId),
            () => CheckIfUserOperationClaimAlreadyExistsAsync(userOperationClaim.UserId, userOperationClaim.OperationClaimId)
        );

        if (result != null)
        {
            return result;
        }

        await _userOperationClaimDal.AddAsync(userOperationClaim);
        return new SuccessResult(Messages.UserOperationClaimAdded);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> DeleteAsync(UserOperationClaim userOperationClaim)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfUserOperationClaimExistsAsync(userOperationClaim.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = await _userOperationClaimDal.GetAsync(u => u.Id == userOperationClaim.Id);
        await _userOperationClaimDal.DeleteAsync(existingClaim);
        return new SuccessResult(Messages.UserOperationClaimDeleted);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> DeleteByUserIdAsync(int userId)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfUserExistsAsync(userId)
        );

        if (result != null)
        {
            return result;
        }

        var claims = await _userOperationClaimDal.GetAllAsync(u => u.UserId == userId);
        foreach (var claim in claims)
        {
            await _userOperationClaimDal.DeleteAsync(claim);
        }
        return new SuccessResult(Messages.UserOperationClaimsDeleted);
    }

    #region Business Rules

    private async Task<IResult> CheckIfUserExistsAsync(int userId)
    {
        var user = await _userDal.GetAsync(u => u.Id == userId);
        if (user == null)
        {
            return new ErrorResult(Messages.UserNotFound);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfOperationClaimExistsAsync(int operationClaimId)
    {
        var claim = await _operationClaimDal.GetAsync(o => o.Id == operationClaimId);
        if (claim == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserOperationClaimAlreadyExistsAsync(int userId, int operationClaimId)
    {
        var claims = await _userOperationClaimDal.GetAllAsync(u => 
            u.UserId == userId && u.OperationClaimId == operationClaimId);
        
        if (claims.Any())
        {
            return new ErrorResult(Messages.UserOperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfUserOperationClaimExistsAsync(int id)
    {
        var claim = await _userOperationClaimDal.GetAsync(u => u.Id == id);
        if (claim == null)
        {
            return new ErrorResult(Messages.UserOperationClaimNotFound);
        }
        return new SuccessResult();
    }

    #endregion
}
