using Core.Entities;

namespace WebUI.Models;

public class Transfer : IEntity
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public int TransferredById { get; set; }
    public DateTime TransferredAt { get; set; }
}
