using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketCategoryDal : EfEntityRepositoryBase<TicketCategory, MsSqlDbContext>, ITicketCategoryDal
{
    public EfTicketCategoryDal(MsSqlDbContext context) : base(context) { }
}
