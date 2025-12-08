using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfOperationClaimDal : EfEntityRepositoryBase<OperationClaim, MsSqlDbContext>, IOperationClaimDal
{
    public EfOperationClaimDal(MsSqlDbContext context) : base(context)
    {
    }
}

