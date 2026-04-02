using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketAssignmentService
    {
        IDataResult<List<Assignment>> GetAll(Expression<Func<Assignment, bool>> filter = null);
        IDataResult<Assignment> GetById(int id);
        IResult Add(Assignment assignment);
        IResult Update(Assignment assignment);
        IResult Delete(Assignment assignment);
    }
}
