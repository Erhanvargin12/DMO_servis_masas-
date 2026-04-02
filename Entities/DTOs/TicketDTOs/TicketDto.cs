using Core.Entities;

namespace Entities.DTOs.TicketDTOs;

public class TicketDto : IDto
{
    public int Id { get; set; }
    public string TicketNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public int CurrentBranchId { get; set; }
    public int? AssignedUserId { get; set; }
    public int StatusId { get; set; }
    public int PriorityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
