using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketAssignmentManager : ITicketAssignmentService
    {
        public IResult Add(Assignment assignment)
        {
            throw new NotImplementedException();
        }

        public IResult Delete(Assignment assignment)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<Assignment>> GetAll(Expression<Func<Assignment, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public IDataResult<Assignment> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IResult Update(Assignment assignment)
        {
            throw new NotImplementedException();
        }
    }
}
