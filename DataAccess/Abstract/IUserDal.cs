using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Abstract;

public interface IUserDal : IEntityRepository<User>
{
    Task<List<OperationClaim>> GetClaimsAsync(User user);
}
