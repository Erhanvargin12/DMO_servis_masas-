using Core.Utilities.Results;
using Entities.Concrete;
using WebUI.Models;

namespace WebUI.Areas.Tickets.Models;

/// <summary>
/// API'den dönen vekalet bilgisi.
/// API hazır olduğunda bu model response'dan doldurulacak.
/// </summary>

public class ProxyEbysUser
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EbysId { get; set; }
    public string DepartmentId { get; set; }
    public string Department { get; set; }
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

    /// <summary>Eğer null ise kendi talepleri, dolu ise ilgili vekaletin talepleri</summary>
    public int? ActiveUserId { get; set; }

    /// <summary>null ise bu kullanıcının aktif vekâleti yok → tek tab gösterilir.</summary>
    public List<User>? ProxyUser { get; set; } = new();

    /// <summary>Kendi tab'ındaki ticket sayısı (badge için).</summary>
    public int OwnCount { get; set; }

    /// <summary>Vekâlet alınan kullanıcıların ticket sayıları (proxyId -> count).</summary>
    public Dictionary<int, int> ProxyCounts { get; set; } = new();
}
