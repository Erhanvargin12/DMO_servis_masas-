using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketTransferService
    {
        IDataResult<List<Transfer>> GetAll(Expression<Func<Transfer, bool>> filter = null);
        IDataResult<Transfer> GetById(int id);
        IResult Add(Transfer transfer);
        IResult Update(Transfer transfer);
        IResult Delete(Transfer transfer);
    }
}
