using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IUserService
{
    Task<IDataResult<List<User>>> GetAllAsync(Expression<Func<User, bool>> filter = null);
    Task<IDataResult<User>> GetUserAsync(Expression<Func<User, bool>> filter = null);
    Task<IResult> AddAsync(User user);
    Task<IResult> UpdateAsync(User user);
    Task<IResult> DeleteAsync(User user);
    Task<List<OperationClaim>> GetClaimsAsync(User user);
}
