using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using System.Linq.Expressions;

namespace Business.Abstract.TicketServices
{
    public interface ITicketAttachmentService
    {
        Task<IDataResult<List<Attachment>>> GetAllAsync(Expression<Func<Attachment, bool>> filter = null);
        Task<IDataResult<Attachment>> GetByIdAsync(int id);
        Task<IResult> AddAsync(Attachment attachment);
        Task<IResult> UpdateAsync(Attachment attachment);
        Task<IResult> DeleteAsync(Attachment attachment);
    }
}
