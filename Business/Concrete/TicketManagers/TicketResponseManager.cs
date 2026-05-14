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
        public IResult Add(TicketResponseDto dto)
        {
            _responseDal.Add(dto.Response);

            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var attachment in dto.Attachments)
                {
                    attachment.TicketId = dto.Response.TicketId;
                    attachment.ResponseId = dto.Response.Id;
                    _attachmentDal.Add(attachment);
                }
            }

            return new SuccessDataResult<TicketResponseDto>(dto, Messages.TicketCreated);
        }

        public IResult Delete(Response response)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<Response>> GetAll(Expression<Func<Response, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public IDataResult<Response> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IResult Update(Response response)
        {
            throw new NotImplementedException();
        }
    }
}
