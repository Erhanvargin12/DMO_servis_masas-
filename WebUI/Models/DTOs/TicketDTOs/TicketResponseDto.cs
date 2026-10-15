using Core.Entities;
using Entities.Concrete.Ticket;

namespace WebUI.Models.DTOs;

public class TicketResponseDto : IDto
{
    public Response? Response { get; set; }
    public List<Attachment>? Attachments { get; set; }
}
