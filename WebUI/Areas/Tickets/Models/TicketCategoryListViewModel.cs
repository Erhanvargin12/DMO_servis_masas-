using Entities.Concrete;
using Entities.Concrete.Ticket;

namespace WebUI.Areas.Tickets.Models;

public class TicketCategoryListViewModel
{
    public List<TicketCategoryItemViewModel> Categories { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int TotalCount => Categories.Count;
}

public class TicketCategoryItemViewModel
{
    public TicketCategory Category { get; set; } = new();
    public List<int> BranchIds { get; set; } = new();
    public int BranchCount => BranchIds.Count;
}
