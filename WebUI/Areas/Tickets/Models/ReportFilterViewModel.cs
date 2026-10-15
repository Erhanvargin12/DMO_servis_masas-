using Entities.Concrete;
using Entities.Concrete.Ticket;
using WebUI.Models.DTOs;

namespace WebUI.Areas.Tickets.Models
{
    public class ReportFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BranchId { get; set; }
        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? AssignedUserId { get; set; }
        public string? SearchTerm { get; set; }

        public List<Ticket> FilteredTickets { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();
        public List<User> Personnel { get; set; } = new();

        // Metrics for summary cards
        public int TotalCount => FilteredTickets.Count;
        public int CompletedCount => FilteredTickets.Count(t => t.StatusId == 5 || t.StatusId == 6);
        public int ActiveCount => FilteredTickets.Count(t => t.StatusId != 6);
        public double SlaRate => TotalCount > 0 ? Math.Round((double)CompletedCount / TotalCount * 100, 1) : 100.0;
    }
}
