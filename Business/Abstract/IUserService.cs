using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IUserService
{
    IDataResult<List<User>> GetAll(Expression<Func<User, bool>> filter = null);
    IDataResult<User> GetUser(Expression<Func<User, bool>> filter = null);
    IResult Add(User user);
    IResult Update(User user);
    IResult Delete(User user);
    List<OperationClaim> GetClaims(User user);
}
