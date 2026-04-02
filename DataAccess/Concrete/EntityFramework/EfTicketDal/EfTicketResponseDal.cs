using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketResponseDal : EfEntityRepositoryBase<Response, MsSqlDbContext>, ITicketResponseDal
{
    public EfTicketResponseDal(MsSqlDbContext context) : base(context) { }
}
