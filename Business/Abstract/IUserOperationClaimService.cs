using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IUserOperationClaimService
{
    IDataResult<List<UserOperationClaim>> GetAll(Expression<Func<UserOperationClaim, bool>> filter = null);
    IDataResult<List<UserOperationClaim>> GetByUserId(int userId);
    IResult Add(UserOperationClaim userOperationClaim);
    IResult Delete(UserOperationClaim userOperationClaim);
    IResult DeleteByUserId(int userId);
}

