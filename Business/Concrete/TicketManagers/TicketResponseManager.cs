using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketResponseManager : ITicketResponseService
    {
        private readonly ITicketResponseDal _responseDal;
        private readonly ITicketAttachmentDal _attachmentDal;

        public TicketResponseManager(ITicketResponseDal responseDal, ITicketAttachmentDal attachmentDal)
        {
            _responseDal = responseDal;
            _attachmentDal = attachmentDal;
        }
        public async Task<IResult> AddAsync(TicketResponseDto dto)
        {
            await _responseDal.AddAsync(dto.Response);

            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var attachment in dto.Attachments)
                {
                    attachment.TicketId = dto.Response.TicketId;
                    attachment.ResponseId = dto.Response.Id;
                    await _attachmentDal.AddAsync(attachment);
                }
            }

            return new SuccessDataResult<TicketResponseDto>(dto, Messages.TicketCreated);
        }

        public Task<IResult> DeleteAsync(Response response)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<Response>>> GetAllAsync(Expression<Func<Response, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<Response>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(Response response)
        {
            throw new NotImplementedException();
        }
    }
}
