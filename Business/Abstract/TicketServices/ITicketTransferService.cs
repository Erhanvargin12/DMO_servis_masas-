using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketTransferService
    {
        Task<IDataResult<List<Transfer>>> GetAllAsync(Expression<Func<Transfer, bool>> filter = null);
        Task<IDataResult<Transfer>> GetByIdAsync(int id);
        Task<IResult> AddAsync(Transfer transfer);
        Task<IResult> UpdateAsync(Transfer transfer);
        Task<IResult> DeleteAsync(Transfer transfer);
    }
}
