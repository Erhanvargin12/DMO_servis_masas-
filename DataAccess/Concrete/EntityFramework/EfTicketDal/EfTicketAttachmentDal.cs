using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketAttachmentDal : EfEntityRepositoryBase<Attachment, MsSqlDbContext>, ITicketAttachmentDal
{
    public EfTicketAttachmentDal(MsSqlDbContext context) : base(context){}
}
