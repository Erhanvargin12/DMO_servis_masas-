using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketAttachmentManager : ITicketAttachmentService
    {
        private readonly ITicketAttachmentDal _ticketAttachmentDal;

        public TicketAttachmentManager(ITicketAttachmentDal ticketAttachmentDal)
        {
            _ticketAttachmentDal = ticketAttachmentDal;
        }

        public async Task<IResult> AddAsync(Attachment attachment)
        {
            await _ticketAttachmentDal.AddAsync(attachment);
            return new SuccessResult(Messages.AttachmentAdded);
        }

        public async Task<IResult> DeleteAsync(Attachment attachment)
        {
            var existing = await _ticketAttachmentDal.GetAsync(a => a.Id == attachment.Id);
            if (existing == null)
                return new ErrorResult(Messages.AttachmentNotFound);

            await _ticketAttachmentDal.DeleteAsync(existing);
            return new SuccessResult(Messages.AttachmentDeleted);
        }

        public async Task<IDataResult<List<Attachment>>> GetAllAsync(Expression<Func<Attachment, bool>> filter = null)
        {
            return new SuccessDataResult<List<Attachment>>(await _ticketAttachmentDal.GetAllAsync(filter));
        }

        public async Task<IDataResult<Attachment>> GetByIdAsync(int id)
        {
            return new SuccessDataResult<Attachment>(await _ticketAttachmentDal.GetAsync(a => a.Id == id));
        }

        public async Task<IResult> UpdateAsync(Attachment attachment)
        {
            var existing = await _ticketAttachmentDal.GetAsync(a => a.Id == attachment.Id);
            if (existing == null)
                return new ErrorResult(Messages.AttachmentNotFound);

            existing.FileName = attachment.FileName;
            existing.FilePath = attachment.FilePath;
            await _ticketAttachmentDal.UpdateAsync(existing);
            return new SuccessResult(Messages.AttachmentUpdated);
        }
    }
}
