using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketAttachmentManager : ITicketAttachmentService
    {
        public IResult Add(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public IResult Delete(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<Attachment>> GetAll(Expression<Func<Attachment, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public IDataResult<Attachment> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IResult Update(Attachment attachment)
        {
            throw new NotImplementedException();
        }
    }
}
