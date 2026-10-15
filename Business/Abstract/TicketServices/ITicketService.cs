using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketService
    {
        Task<IDataResult<List<Ticket>>> GetAllAsync(Expression<Func<Ticket, bool>> filter = null);
        Task<IDataResult<Ticket>> GetByIdAsync(int id);
        Task<IDataResult<TicketDto>> GetTicketDtoByIdAsync(int id);
        Task<IDataResult<List<Ticket>>> GetByBranchAsync(int branchId);
        Task<IDataResult<List<Ticket>>> GetAssignedAsync(int userId);
        Task<IDataResult<TicketDto>> AddAsync(TicketDto ticket);
        Task<IResult> UpdateAsync(Ticket ticket);
        Task<IResult> DeleteAsync(int id);
    }
}
