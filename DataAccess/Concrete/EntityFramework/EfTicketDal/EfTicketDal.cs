using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;
public class EfTicketDal : EfEntityRepositoryBase<Ticket, MsSqlDbContext>, ITicketDal
{
    public EfTicketDal(MsSqlDbContext context) : base(context) { }
}
