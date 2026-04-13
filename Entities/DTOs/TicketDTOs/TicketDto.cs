using Core.Entities;
using Entities.Concrete.Ticket;

namespace Entities.DTOs.TicketDTOs;

public class TicketDto : IDto
{
    public Ticket? Ticket { get; set; }
    public Assignment? Assignment { get; set; }
    public List<Attachment>? Attachments { get; set; }
    public List<Response>? Responses { get; set; }
}
