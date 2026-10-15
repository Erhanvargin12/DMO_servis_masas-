using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketAssignmentManager : ITicketAssignmentService
    {
        private readonly ITicketAssignmentDal _assignmentDal;
        private readonly ITicketResponseDal _responseDal;
        private readonly ITicketDal _ticketDal;

        public TicketAssignmentManager(ITicketAssignmentDal assignmentDal, ITicketResponseDal responseDal, ITicketDal ticketDal)
        {
            _assignmentDal = assignmentDal;
            _responseDal = responseDal;
            _ticketDal = ticketDal;
        }

        public async Task<IResult> AddAsync(Assignment assignment)
        {
            await _assignmentDal.AddAsync(assignment);

            var ticket = await _ticketDal.GetAsync(t => t.Id == assignment.TicketId);
            if (ticket != null)
            {
                ticket.AssignedUserId = assignment.AssignedToId;
                ticket.StatusId = 2; // Atandı
                ticket.UpdatedAt = DateTime.Now;
                await _ticketDal.UpdateAsync(ticket);
            }

            if (!string.IsNullOrEmpty(assignment.Description))
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
