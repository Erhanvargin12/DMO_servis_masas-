using Core.DataAccess;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace DataAccess.Abstract.TicketDal;

public interface ITicketDal : IEntityRepository<Ticket>
{
    Task<TicketDto?> GetTicketDtoByIdAsync(int id);
}
