using Core.Entities;

namespace Entities.Concrete.Ticket;

public class Ticket : IEntity
{
    public int Id { get; set; }
    public string TicketNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public int CurrentBranchId { get; set; }
    public int? AssignedUserId { get; set; }
    public int? StatusId { get; set; }
    public int PriorityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }


    public Assignment? Assignment { get; set; }
    public List<Attachment>? Attachment { get; set; }
    public List<Response>? Responses { get; set; }
}
