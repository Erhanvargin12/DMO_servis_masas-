using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketTransferManager : ITicketTransferService
    {
        public IResult Add(Transfer transfer)
        {
            throw new NotImplementedException();
        }

        public IResult Delete(Transfer transfer)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<Transfer>> GetAll(Expression<Func<Transfer, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public IDataResult<Transfer> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IResult Update(Transfer transfer)
        {
            throw new NotImplementedException();
        }
    }
}
