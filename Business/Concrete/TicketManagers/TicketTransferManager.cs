using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketTransferManager : ITicketTransferService
    {
        public Task<IResult> AddAsync(Transfer transfer)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(Transfer transfer)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<Transfer>>> GetAllAsync(Expression<Func<Transfer, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<Transfer>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(Transfer transfer)
        {
            throw new NotImplementedException();
        }
    }
}
