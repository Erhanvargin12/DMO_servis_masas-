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

    [SecuredOperation("Admin")]
    public IDataResult<List<OperationClaim>> GetAll(Expression<Func<OperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<OperationClaim>>(_operationClaimDal.GetAll(filter), Messages.OperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public IDataResult<OperationClaim> GetById(int id)
    {
        var result = _operationClaimDal.Get(o => o.Id == id);
        if (result == null)
        {
            return new ErrorDataResult<OperationClaim>(Messages.OperationClaimNotFound);
        }
        return new SuccessDataResult<OperationClaim>(result);
    }

    [SecuredOperation("Admin")]
    public IResult Add(OperationClaim operationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfOperationClaimNameExists(operationClaim.Name)
        );

        if (result != null)
        {
            return result;
        }

        _operationClaimDal.Add(operationClaim);
        return new SuccessResult(Messages.OperationClaimAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Update(OperationClaim operationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfOperationClaimExists(operationClaim.Id),
            CheckIfOperationClaimNameExistsForUpdate(operationClaim.Id, operationClaim.Name)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = _operationClaimDal.Get(o => o.Id == operationClaim.Id);
        existingClaim.Name = operationClaim.Name;
        existingClaim.Description = operationClaim.Description;

        _operationClaimDal.Update(existingClaim);
        return new SuccessResult(Messages.OperationClaimUpdated);
    }

    [SecuredOperation("Admin")]
    public IResult Delete(OperationClaim operationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfOperationClaimExists(operationClaim.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = _operationClaimDal.Get(o => o.Id == operationClaim.Id);
        _operationClaimDal.Delete(existingClaim);
        return new SuccessResult(Messages.OperationClaimDeleted);
    }

    #region Business Rules

    private IResult CheckIfOperationClaimNameExists(string name)
    {
        var result = _operationClaimDal.GetAll(o => o.Name == name).Any();
        if (result)
        {
            return new ErrorResult(Messages.OperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfOperationClaimNameExistsForUpdate(int id, string name)
    {
        var result = _operationClaimDal.GetAll(o => o.Name == name && o.Id != id).Any();
        if (result)
        {
            return new ErrorResult(Messages.OperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfOperationClaimExists(int id)
    {
        var result = _operationClaimDal.Get(o => o.Id == id);
        if (result == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }

    #endregion
}
