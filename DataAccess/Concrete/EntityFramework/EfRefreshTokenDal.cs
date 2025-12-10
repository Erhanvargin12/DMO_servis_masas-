using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfRefreshTokenDal : EfEntityRepositoryBase<RefreshToken, MsSqlDbContext>, IRefreshTokenDal
    {
        public EfRefreshTokenDal(MsSqlDbContext context) : base(context) { }
    }
}
