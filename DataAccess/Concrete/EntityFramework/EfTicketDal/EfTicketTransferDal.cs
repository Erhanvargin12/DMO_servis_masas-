using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketTransferDal : EfEntityRepositoryBase<Transfer, MsSqlDbContext>, ITicketTransferDal
{
    public EfTicketTransferDal(MsSqlDbContext context) : base(context) { }
}