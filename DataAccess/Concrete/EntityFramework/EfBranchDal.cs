using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

public class EfBranchDal : EfEntityRepositoryBase<Branch, MsSqlDbContext>, IBranchDal
{
    public EfBranchDal(MsSqlDbContext context) : base(context) { }
}
