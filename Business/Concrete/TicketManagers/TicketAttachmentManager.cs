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

        public IResult Add(Attachment attachment)
        {
            _ticketAttachmentDal.Add(attachment);
            return new SuccessResult(Messages.AttachmentAdded);
        }

        public IResult Delete(Attachment attachment)
        {
            var existing = _ticketAttachmentDal.Get(a => a.Id == attachment.Id);
            if (existing == null)
                return new ErrorResult(Messages.AttachmentNotFound);

            _ticketAttachmentDal.Delete(existing);
            return new SuccessResult(Messages.AttachmentDeleted);
        }

        public IDataResult<List<Attachment>> GetAll(Expression<Func<Attachment, bool>> filter = null)
        {
            return new SuccessDataResult<List<Attachment>>(_ticketAttachmentDal.GetAll(filter));
        }

        public IDataResult<Attachment> GetById(int id)
        {
            return new SuccessDataResult<Attachment>(_ticketAttachmentDal.Get(a => a.Id == id));
        }

        public IResult Update(Attachment attachment)
        {
            var existing = _ticketAttachmentDal.Get(a => a.Id == attachment.Id);
            if (existing == null)
                return new ErrorResult(Messages.AttachmentNotFound);

            existing.FileName = attachment.FileName;
            existing.FilePath = attachment.FilePath;
            _ticketAttachmentDal.Update(existing);
            return new SuccessResult(Messages.AttachmentUpdated);
        }
    }
}
