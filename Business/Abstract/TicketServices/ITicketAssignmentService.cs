using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketAssignmentService
    {
        Task<IDataResult<List<Assignment>>> GetAllAsync(Expression<Func<Assignment, bool>> filter = null);
        Task<IDataResult<Assignment>> GetByIdAsync(int id);
        Task<IResult> AddAsync(Assignment assignment);
        Task<IResult> UpdateAsync(Assignment assignment);
        Task<IResult> DeleteAsync(Assignment assignment);
    }
}
