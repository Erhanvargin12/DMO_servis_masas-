using Entities.Concrete;
using Entities.Concrete.Ticket;

namespace WebUI.Areas.Tickets.Models;

public class TicketDetailViewModel
{
    public Ticket?               Ticket      { get; set; }
    public List<User>            Personnel   { get; set; } = new();
    public List<Response>        Responses   { get; set; } = new();
    public List<Attachment>      Attachments { get; set; } = new();
    public List<TicketHistoryItem> History   { get; set; } = new();

    public string StatusLabel => Ticket?.StatusId switch
    {
        1 => "Açık",
        2 => "Atandı",
        3 => "İşlemde",
        4 => "Beklemede",
        5 => "Çözüldü",
        6 => "Kapatıldı",
        _ => "—"
    };

    public string StatusBadge => Ticket?.StatusId switch
    {
        1 => "badge-status-open",
        2 => "badge-status-assigned",
        3 => "badge-status-progress",
        4 => "badge-status-waiting",
        5 => "badge-status-resolved",
        6 => "badge-status-closed",
        _ => ""
    };

    public string PriorityLabel => Ticket?.PriorityId switch
    {
        1 => "Düşük",
        2 => "Orta",
        3 => "Yüksek",
        4 => "Kritik",
        _ => "—"
    };

    public string PriorityBadge => Ticket?.PriorityId switch
    {
        1 => "badge-priority-low",
        2 => "badge-priority-medium",
        3 => "badge-priority-high",
        4 => "badge-priority-critical",
        _ => ""
    };
}

public class TicketHistoryItem
{
    public DateTime  Date      { get; set; }
    public string    ActorName { get; set; } = "";
    public string    Type      { get; set; } = ""; // created | status | assigned | replied | transferred | closed | note
    public string    Title     { get; set; } = "";
    public string?   Detail    { get; set; }
}

