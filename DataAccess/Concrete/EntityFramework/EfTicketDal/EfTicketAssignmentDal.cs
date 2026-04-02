using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketAssignmentDal : EfEntityRepositoryBase<Assignment, MsSqlDbContext>, ITicketAssignmentDal
{
    public EfTicketAssignmentDal(MsSqlDbContext context) : base(context){}
}
