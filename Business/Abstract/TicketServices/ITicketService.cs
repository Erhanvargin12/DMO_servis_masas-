using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketService
    {
        IDataResult<List<Ticket>> GetAll(Expression<Func<Ticket, bool>> filter = null);
        IDataResult<Ticket> GetById(int id);
        IResult Add(Ticket ticket);
        IResult Update(Ticket ticket);
        IResult Delete(int id);
    }
}
