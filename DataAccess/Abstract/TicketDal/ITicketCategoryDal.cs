using Core.DataAccess;
using Entities.Concrete.Ticket;

namespace DataAccess.Abstract.TicketDal;

public interface ITicketCategoryDal : IEntityRepository<TicketCategory> { }
