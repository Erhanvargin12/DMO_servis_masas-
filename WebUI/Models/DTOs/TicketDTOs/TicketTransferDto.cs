using Core.Entities;

namespace WebUI.Models.DTOs.TicketDTOs;

public class TicketTransferDto : IDto
{
    public int TicketId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public int TransferredById { get; set; }
    public string? Note { get; set; }
}
