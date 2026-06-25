using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Concrete;

public class OperationClaimManager : IOperationClaimService
{
    private readonly IOperationClaimDal _operationClaimDal;

    public OperationClaimManager(IOperationClaimDal operationClaimDal)
    {
        _operationClaimDal = operationClaimDal;
    }

    //[SecuredOperation("Admin")]
    public async Task<IDataResult<List<OperationClaim>>> GetAllAsync(Expression<Func<OperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<OperationClaim>>(await _operationClaimDal.GetAllAsync(filter), Messages.OperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public async Task<IDataResult<OperationClaim>> GetByIdAsync(int id)
    {
        var result = await _operationClaimDal.GetAsync(o => o.Id == id);
        if (result == null)
        {
            return new ErrorDataResult<OperationClaim>(Messages.OperationClaimNotFound);
        }
        return new SuccessDataResult<OperationClaim>(result);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> AddAsync(OperationClaim operationClaim)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfOperationClaimNameExistsAsync(operationClaim.Name)
        );

        if (result != null)
        {
            return result;
        }

        await _operationClaimDal.AddAsync(operationClaim);
        return new SuccessResult(Messages.OperationClaimAdded);
    }

    [SecuredOperation("Admin")]
    public async Task<IResult> UpdateAsync(OperationClaim operationClaim)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfOperationClaimExistsAsync(operationClaim.Id),
            () => CheckIfOperationClaimNameExistsForUpdateAsync(operationClaim.Id, operationClaim.Name)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = await _operationClaimDal.GetAsync(o => o.Id == operationClaim.Id);
        existingClaim.Name = operationClaim.Name;
        existingClaim.Description = operationClaim.Description;

        await _operationClaimDal.UpdateAsync(existingClaim);
        return new SuccessResult(Messages.OperationClaimUpdated);
    }

    //[SecuredOperation("Admin")]
    public async Task<IResult> DeleteAsync(OperationClaim operationClaim)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfOperationClaimExistsAsync(operationClaim.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = await _operationClaimDal.GetAsync(o => o.Id == operationClaim.Id);
        await _operationClaimDal.DeleteAsync(existingClaim);
        return new SuccessResult(Messages.OperationClaimDeleted);
    }

    #region Business Rules

    private async Task<IResult> CheckIfOperationClaimNameExistsAsync(string name)
    {
        var claims = await _operationClaimDal.GetAllAsync(o => o.Name == name);
        if (claims.Any())
        {
            return new ErrorResult(Messages.OperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfOperationClaimNameExistsForUpdateAsync(int id, string name)
    {
        var claims = await _operationClaimDal.GetAllAsync(o => o.Name == name && o.Id != id);
        if (claims.Any())
        {
            return new ErrorResult(Messages.OperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private async Task<IResult> CheckIfOperationClaimExistsAsync(int id)
    {
        var result = await _operationClaimDal.GetAsync(o => o.Id == id);
        if (result == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }

    #endregion
}
