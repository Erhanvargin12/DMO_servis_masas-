using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketResponseService
    {
        Task<IDataResult<List<Response>>> GetAllAsync(Expression<Func<Response, bool>> filter = null);
        Task<IDataResult<Response>> GetByIdAsync(int id);
        Task<IResult> AddAsync(TicketResponseDto dto);
        Task<IResult> UpdateAsync(Response response);
        Task<IResult> DeleteAsync(Response response);
    }
}
