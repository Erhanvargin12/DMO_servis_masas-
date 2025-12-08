using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfMenuOperationClaimDal : EfEntityRepositoryBase<MenuOperationClaim, MsSqlDbContext>, IMenuOperationClaimDal
{
    public EfMenuOperationClaimDal(MsSqlDbContext context) : base(context)
    {
    }
}

