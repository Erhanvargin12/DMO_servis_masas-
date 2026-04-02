using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Concrete.EntityFramework;

public class EfMenuDal : EfEntityRepositoryBase<Menu, MsSqlDbContext>, IMenuDal
{
    private readonly MsSqlDbContext _context;

    public EfMenuDal(MsSqlDbContext context) : base(context)
    {
        _context = context;
    }
}

