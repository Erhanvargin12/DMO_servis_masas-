using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfCompanyDal : EfEntityRepositoryBase<Company, MsSqlDbContext>, ICompanyDal
{
    public EfCompanyDal(MsSqlDbContext context) : base(context) { }
}
