using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IUserOperationClaimService
{
    Task<IDataResult<List<UserOperationClaim>>> GetAllAsync(Expression<Func<UserOperationClaim, bool>> filter = null);
    Task<IDataResult<List<UserOperationClaim>>> GetByUserIdAsync(int userId);
    Task<IResult> AddAsync(UserOperationClaim userOperationClaim);
    Task<IResult> DeleteAsync(UserOperationClaim userOperationClaim);
    Task<IResult> DeleteByUserIdAsync(int userId);
}

