using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IOperationClaimService
{
    IDataResult<List<OperationClaim>> GetAll(Expression<Func<OperationClaim, bool>> filter = null);
    IDataResult<OperationClaim> GetById(int id);
    IResult Add(OperationClaim operationClaim);
    IResult Update(OperationClaim operationClaim);
    IResult Delete(OperationClaim operationClaim);
}

