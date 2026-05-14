using Core.Entities;

namespace WebUI.Models.DTOs;

public class TicketResponseDto : IDto
{
    public Response? Response { get; set; }
    public List<Attachment>? Attachments { get; set; }
}
