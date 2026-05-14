using Core.Entities;
using Entities.Concrete.Ticket;

namespace Entities.DTOs.TicketDTOs;

public class TicketResponseDto : IDto
{
    public Response? Response { get; set; }
    public List<Attachment>? Attachments { get; set; }
}
