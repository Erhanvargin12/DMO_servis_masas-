using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Concrete.EntityFramework;
public class EfUserDal : EfEntityRepositoryBase<User, MsSqlDbContext>, IUserDal
{

    public EfUserDal(MsSqlDbContext context) : base(context){}


    public List<OperationClaim> GetClaims(User user)
    {
        var result = from operationClaim in _context.OperationClaims
                     join userOperationClaim in _context.UserOperationClaims
                         on operationClaim.Id equals userOperationClaim.OperationClaimId
                     where userOperationClaim.UserId == user.Id
                     select new OperationClaim
                     {
                         Id = operationClaim.Id,
                         Name = operationClaim.Name,
                         Description = operationClaim.Description
                     };

        return result.ToList();
    }

}
