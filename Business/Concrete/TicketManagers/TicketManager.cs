using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketManager : ITicketService
    {
        private readonly ITicketDal _ticketDal;
        private readonly ITicketAttachmentDal _attachmentDal;

        public TicketManager(ITicketDal ticketDal, ITicketAttachmentDal attachmentDal)
        {
            _ticketDal = ticketDal;
            _attachmentDal = attachmentDal;
        }

        public async Task<IDataResult<TicketDto>> AddAsync(TicketDto dto)
        {
            await _ticketDal.AddAsync(dto.Ticket);

            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var attachment in dto.Attachments)
                {
                    attachment.TicketId = dto.Ticket.Id;
                    await _attachmentDal.AddAsync(attachment);
                }
            }

            return new SuccessDataResult<TicketDto>(dto, Messages.TicketCreated);
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            IResult result = await BusinessRules.RunAsync(() => CheckIfTicketExistsAsync(id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = await _ticketDal.GetAsync(u => u.Id == id);
            await _ticketDal.DeleteAsync(existingTicket);
            return new SuccessResult(Messages.TicketDeleted);
        }

        public async Task<IDataResult<List<Ticket>>> GetAllAsync(Expression<Func<Ticket, bool>> filter = null)
        {
            return new SuccessDataResult<List<Ticket>>(await _ticketDal.GetAllAsync(filter), Messages.TicketsListed);
        }

        public async Task<IDataResult<Ticket>> GetByIdAsync(int id)
        {
            return new SuccessDataResult<Ticket>(await _ticketDal.GetAsync(t => t.Id == id), Messages.TicketsListed);
        }

        public async Task<IDataResult<TicketDto>> GetTicketDtoByIdAsync(int id)
        {
            return new SuccessDataResult<TicketDto>(await _ticketDal.GetTicketDtoByIdAsync(id), Messages.TicketsListed);
        }

        public async Task<IResult> UpdateAsync(Ticket ticket)
        {
            IResult result = await BusinessRules.RunAsync(() => CheckIfTicketExistsAsync(ticket.Id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = await _ticketDal.GetAsync(u => u.Id == ticket.Id);
            existingTicket.Description = ticket.Description;
            await _ticketDal.UpdateAsync(existingTicket);
            return new SuccessResult(Messages.TicketUpdated);
        }

        private async Task<IResult> CheckIfTicketExistsAsync(int id)
        {
            var result = await _ticketDal.GetAsync(u => u.Id == id);
            if (result == null)
            {
                return new ErrorResult(Messages.TicketNotFound);
            }
            return new SuccessResult();
        }
    }
}
