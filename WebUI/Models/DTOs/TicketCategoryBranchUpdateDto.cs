namespace WebUI.Models.DTOs;

public class TicketCategoryBranchUpdateDto
{
    public int CategoryId { get; set; }
    public List<int>? BranchIds { get; set; }
}
