using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketResponseManager : ITicketResponseService
    {
        public IResult Add(Response response)
        {
            throw new NotImplementedException();
        }

        public IResult Delete(Response response)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<Response>> GetAll(Expression<Func<Response, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public IDataResult<Response> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IResult Update(Response response)
        {
            throw new NotImplementedException();
        }
    }
}
