using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketResponseService
    {
        IDataResult<List<Response>> GetAll(Expression<Func<Response, bool>> filter = null);
        IDataResult<Response> GetById(int id);
        IResult Add(TicketResponseDto dto);
        IResult Update(Response response);
        IResult Delete(Response response);
    }
}
