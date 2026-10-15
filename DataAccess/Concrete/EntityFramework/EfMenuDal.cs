using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Concrete.EntityFramework;

public class EfMenuDal : EfEntityRepositoryBase<Menu, MsSqlDbContext>, IMenuDal
{
    public EfMenuDal(MsSqlDbContext context) : base(context)
    {
    }
}

