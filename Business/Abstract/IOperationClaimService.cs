using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IOperationClaimService
{
    Task<IDataResult<List<OperationClaim>>> GetAllAsync(Expression<Func<OperationClaim, bool>> filter = null);
    Task<IDataResult<OperationClaim>> GetByIdAsync(int id);
    Task<IResult> AddAsync(OperationClaim operationClaim);
    Task<IResult> UpdateAsync(OperationClaim operationClaim);
    Task<IResult> DeleteAsync(OperationClaim operationClaim);
}

