using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketManager : ITicketService
    {
        private ITicketDal _ticketDal;

        public TicketManager(ITicketDal ticketDal)
        {
            _ticketDal = ticketDal;
        }

        public IResult Add(Ticket ticket)
        {
            _ticketDal.Add(ticket);
            return new SuccessResult(Messages.TicketCreated);
        }

        public IResult Delete(int id)
        {
            IResult result = BusinessRules.Run(CheckIfTicketExists(id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = _ticketDal.Get(u => u.Id == id);
            _ticketDal.Delete(existingTicket);
            return new SuccessResult(Messages.TicketDeleted);
        }

        public IDataResult<List<Ticket>> GetAll(Expression<Func<Ticket, bool>> filter = null)
        {
            return new SuccessDataResult<List<Ticket>>(_ticketDal.GetAll(filter), Messages.TicketsListed);
        }

        public IDataResult<Ticket> GetById(int id)
        {
            return new SuccessDataResult<Ticket>(_ticketDal.Get(t=>t.Id == id), Messages.TicketsListed);
        }

        public IResult Update(Ticket ticket)
        {
            IResult result = BusinessRules.Run(CheckIfTicketExists(ticket.Id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = _ticketDal.Get(u => u.Id == ticket.Id);

            existingTicket.Description = ticket.Description;

            _ticketDal.Update(existingTicket);
            return new SuccessResult(Messages.TicketUpdated);
        }

        private IResult CheckIfTicketExists(int id)
        {
            var result = _ticketDal.Get(u => u.Id == id);
            if (result == null)
            {
                return new ErrorResult(Messages.TicketNotFound);
            }
            return new SuccessResult();
        }
    }
}
