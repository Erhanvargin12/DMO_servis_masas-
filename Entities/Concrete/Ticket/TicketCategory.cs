using Core.Entities;

namespace Entities.Concrete.Ticket;

/// <summary>Problem / talep kategorisi</summary>
public class TicketCategory : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
