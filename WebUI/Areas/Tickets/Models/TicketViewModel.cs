using Core.Utilities.Results;
using WebUI.Models;

namespace WebUI.Areas.Tickets.Models;

/// <summary>
/// API'den dönen vekalet bilgisi.
/// API hazır olduğunda bu model response'dan doldurulacak.
/// </summary>
public class DelegationInfo
{
    public int    DelegatorId   { get; set; }
    public string DelegatorName { get; set; } = "";

    /// <summary>
    /// "manager"  → vekâlet veren müdür; vekil şubenin TÜM ticketlarını görür.
    /// "personnel"→ vekâlet veren personel; vekil sadece o kişinin ticketlarını görür.
    /// </summary>
    public string DelegationType { get; set; } = "manager";

    /// <summary>Müdür vekâletinde şube adı.</summary>
    public string BranchName { get; set; } = "";
}

public class TicketViewModel
{
    public IDataResult<List<Ticket>>? Ticket { get; set; }
    public string? SearchTerm    { get; set; }
    public string? StatusFilter  { get; set; }
    public string? PriorityFilter{ get; set; }

    // Sayfalama
    public int PageSize { get; set; } = 10;

    // Aktif tab'ın gösterdiği liste (controller tarafından doldurulur)
    public List<Ticket> FilteredTickets { get; set; } = new();

    public int TotalCount => Ticket?.Data?.Count ?? 0;

    // ── Vekalet ──────────────────────────────────────────────────────────────

    /// <summary>"own" | "delegated"</summary>
    public string ActiveTab { get; set; } = "own";

    /// <summary>null ise bu kullanıcının aktif vekâleti yok → tek tab gösterilir.</summary>
    public DelegationInfo? Delegation { get; set; }

    /// <summary>Kendi tab'ındaki ticket sayısı (badge için).</summary>
    public int OwnCount       { get; set; }

    /// <summary>Vekâlet tab'ındaki ticket sayısı (badge için).</summary>
    public int DelegatedCount { get; set; }
}
