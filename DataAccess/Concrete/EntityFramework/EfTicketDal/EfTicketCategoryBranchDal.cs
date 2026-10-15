using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;

public class EfTicketCategoryBranchDal : EfEntityRepositoryBase<TicketCategoryBranch, MsSqlDbContext>, ITicketCategoryBranchDal
{
    public EfTicketCategoryBranchDal(MsSqlDbContext context) : base(context) { }
}
