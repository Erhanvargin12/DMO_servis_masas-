using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketAssignmentManager : ITicketAssignmentService
    {
        private readonly ITicketAssignmentDal _assignmentDal;
        private readonly ITicketResponseDal _responseDal;

        public TicketAssignmentManager(ITicketAssignmentDal assignmentDal, ITicketResponseDal responseDal)
        {
            _assignmentDal = assignmentDal;
            _responseDal = responseDal;
        }
        public IResult Add(Assignment assignment)
        {
            _assignmentDal.Add(assignment);

            if (!assignment.Description.IsNullOrEmpty())
            {
                _responseDal.Add(new Response
                {
                    TicketId = assignment.TicketId,
                    CreatedAt = DateTime.Now,
                    IsInternal = true,
                    Message = assignment.Description,
                    SenderId = assignment.AssignedById
                });
            }
            return new SuccessResult(Messages.TicketCreated);
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
