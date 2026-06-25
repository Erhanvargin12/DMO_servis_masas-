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
        public async Task<IResult> AddAsync(Assignment assignment)
        {
            await _assignmentDal.AddAsync(assignment);

            if (!assignment.Description.IsNullOrEmpty())
            {
                await _responseDal.AddAsync(new Response
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

        public Task<IResult> DeleteAsync(Assignment assignment)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<Assignment>>> GetAllAsync(Expression<Func<Assignment, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<Assignment>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(Assignment assignment)
        {
            throw new NotImplementedException();
        }
    }
}
