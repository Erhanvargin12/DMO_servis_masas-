using Core.Entities;

namespace WebUI.Models.DTOs;

public class TicketDto : IDto
{
    public Ticket? Ticket { get; set; }
    public Assignment? Assignment { get; set; }
    public List<Attachment>? Attachments { get; set; }
    public List<Response>? Responses { get; set; }
    public IFormFileCollection? FormFiles { get; set; }
}
