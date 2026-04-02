using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketAttachmentService
    {
        IDataResult<List<Attachment>> GetAll(Expression<Func<Attachment, bool>> filter = null);
        IDataResult<Attachment> GetById(int id);
        IResult Add(Attachment attachment);
        IResult Update(Attachment attachment);
        IResult Delete(Attachment attachment);
    }
}
