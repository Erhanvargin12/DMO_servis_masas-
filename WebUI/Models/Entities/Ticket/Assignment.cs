using Core.Entities;

namespace WebUI.Models;

public class Assignment : IEntity
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AssignedById { get; set; }
    public int AssignedToId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? Description { get; set; }
}
