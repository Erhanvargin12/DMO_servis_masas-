using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IUserService
{
    Task<IDataResult<List<User>>> GetAll(Expression<Func<User, bool>> filter = null);
    Task<IDataResult<User>> GetUser(Expression<Func<User, bool>> filter = null);
    Task<IResult> Add(User user);
    Task<IResult> Update(User user);
    Task<IResult> Delete(User user);
    Task<List<OperationClaim>> GetClaims(User user);
}
