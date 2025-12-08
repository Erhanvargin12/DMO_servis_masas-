using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfUserOperationClaimDal : EfEntityRepositoryBase<UserOperationClaim, MsSqlDbContext>, IUserOperationClaimDal
{
    public EfUserOperationClaimDal(MsSqlDbContext context) : base(context)
    {
    }
}

