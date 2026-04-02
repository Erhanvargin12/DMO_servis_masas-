using Entities.Concrete;
using Entities.Concrete.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;

namespace WebUI.Areas.Talepler.Controllers;

[Area("Tickets")]
[Authorize]
public class TicketController : BaseController
{
    public TicketController(IApiClient apiClient) : base(apiClient) { }

    // GET /Tickets/Ticket/Index
    public async Task<IActionResult> Index(
        string? search, string? status, string? priority, string? tab)
    {
        tab ??= "own";

        var model    = new TicketViewModel();
        var response = await SendRequest<List<Ticket>>("ticket/getall");

        model.Ticket          = response;
        model.SearchTerm      = search;
        model.StatusFilter    = status;
        model.PriorityFilter  = priority;
        model.ActiveTab       = tab;

        // ── Mock vekalet bilgisi (API bağlanana kadar) ────────────────────────
        // API hazır olduğunda: kullanıcının aktif vekâletleri /delegation/getactive
        // endpoint'inden çekilecek ve bu blok kaldırılacak.
        var mockDelegation = new DelegationInfo
        {
            DelegatorId    = 5,
            DelegatorName  = "Ahmet Yılmaz",
            DelegationType = "manager",      // "manager" | "personnel"
            BranchName     = "IT Müdürlüğü"
        };
        model.Delegation = mockDelegation;
        // ─────────────────────────────────────────────────────────────────────

        // Kendi ticketları
        var ownList = response?.Data ?? new List<Ticket>();

        // Vekâlet ticketları (mock) — API gelince /ticket/getdelegated/{delegatorId}
        var delegatedList = GetMockDelegatedTickets();

        // Sayaçlar (filtreden önce)
        model.OwnCount       = ownList.Count;
        model.DelegatedCount = delegatedList.Count;

        // Aktif tab'a göre filtrelenecek liste
        var list = tab == "delegated" ? delegatedList : ownList;

        // ── Filtreler ─────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            list = list.Where(t =>
                t.Title.ToLowerInvariant().Contains(s) ||
                t.TicketNo.ToLowerInvariant().Contains(s) ||
                t.Description.ToLowerInvariant().Contains(s)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out var sid))
            list = list.Where(t => t.StatusId == sid).ToList();

        if (!string.IsNullOrWhiteSpace(priority) && int.TryParse(priority, out var pid))
            list = list.Where(t => t.PriorityId == pid).ToList();

        model.FilteredTickets = list;

        // Sidebar badge için sayıyı session'a kaydet (her ticket sayfasında güncellenir)
        HttpContext.Session.SetInt32("SidebarTicketCount", model.OwnCount);

        return View(model);
    }

    // GET /Tickets/Ticket/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var ticketResponse      = await SendRequest<Ticket>($"ticket/getbyid/{id}");
        var usersResponse       = await SendRequest<List<User>>("user/getall");
        var responsesResponse   = await SendRequest<List<Response>>($"ticketresponse/getbyticketid/{id}");
        var attachmentsResponse = await SendRequest<List<Attachment>>($"ticketattachment/getbyticketid/{id}");

        // ── Mock veri (API bağlanana kadar) ──────────────────────────────────
        var ticket      = ticketResponse?.Data;
        var personnel   = usersResponse?.Data?.Where(u => u.Status).ToList() ?? new List<User>();
        var responses   = responsesResponse?.Data ?? new List<Response>();
        var attachments = attachmentsResponse?.Data ?? new List<Attachment>();

        if (ticket == null)
        {
            ticket = new Ticket
            {
                Id = id, TicketNo = $"TKT-2026-{id:000}",
                Title       = "Yazıcı Bağlantı Sorunu — 3. Kat Muhasebe",
                Description = "3. kattaki muhasebe departmanında bulunan HP LaserJet 408dn model yazıcı ağa bağlanamıyor. " +
                              "Diğer bilgisayarlardan da erişilemiyor. Yazıcı ekranında 'Network Error' hatası görünüyor. " +
                              "Sorun dün sabahtan beri devam ediyor, acil çözüm gerekmektedir.",
                StatusId = 3, PriorityId = 3, CreatedById = 1, AssignedUserId = 2,
                CurrentBranchId = 1,
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = DateTime.Now.AddHours(-2)
            };
        }

        if (!personnel.Any())
        {
            personnel = new List<User>
            {
                new() { Id = 1, FirstName = "Ahmet",  LastName = "Yılmaz", Status = true },
                new() { Id = 2, FirstName = "Mehmet", LastName = "Kaya",   Status = true },
                new() { Id = 3, FirstName = "Selin",  LastName = "Demir",  Status = true },
                new() { Id = 4, FirstName = "Fatma",  LastName = "Şahin",  Status = true },
            };
        }

        if (!attachments.Any())
        {
            attachments = new List<Attachment>
            {
                new() { Id = 1, FileName = "network-error-ekrani.jpg",        FilePath = "#", CreatedAt = DateTime.Now.AddDays(-3), ResponseId = 1, UploadedById = 1 },
                new() { Id = 2, FileName = "yazici-konfigurasyonu.pdf",        FilePath = "#", CreatedAt = DateTime.Now.AddDays(-3), ResponseId = 1, UploadedById = 1 },
                new() { Id = 3, FileName = "ip-ayarlari-dokumantasyon.docx",   FilePath = "#", CreatedAt = DateTime.Now.AddDays(-2), ResponseId = 2, UploadedById = 3 },
            };
        }

        if (!responses.Any())
        {
            responses = new List<Response>
            {
                new()
                {
                    Id = 1, TicketId = id, SenderId = 1, IsInternal = false,
                    Message   = "Yazıcı IP adresini kontrol ettim, DHCP'den farklı bir IP almış. Statik IP atanması gerekiyor. Müdahale için onay bekliyorum.",
                    CreatedAt = DateTime.Now.AddDays(-2).AddHours(-5),
                    Attachments = attachments.Where(a => a.ResponseId == 1).ToList()
                },
                new()
                {
                    Id = 2, TicketId = id, SenderId = 3, IsInternal = false,
                    Message   = "Statik IP ataması yapıldı (192.168.1.45). Şimdi yazıcı ağda görünüyor ancak bazı bilgisayarlardan print kuyruğunda sorun var. Sürücü güncellemesi gerekebilir.",
                    CreatedAt = DateTime.Now.AddDays(-1).AddHours(-2),
                    Attachments = attachments.Where(a => a.ResponseId == 2).ToList()
                },
                new()
                {
                    Id = 3, TicketId = id, SenderId = 2, IsInternal = true,
                    Message   = "Tüm bilgisayarlarda sürücüler güncellendi. Test çıktısı başarılı. Sorun çözüldü gibi görünüyor, kullanıcı onayı bekleniyor.",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    Attachments = new List<Attachment>()
                },
            };
        }
        // ── Mock geçmiş (API bağlanana kadar) ────────────────────────────────
        var history = new List<TicketHistoryItem>
        {
            new() { Date = ticket.CreatedAt,                          Type = "created",     ActorName = "Ahmet Yılmaz",  Title = "Ticket oluşturuldu",             Detail = null },
            new() { Date = ticket.CreatedAt.AddMinutes(5),            Type = "status",      ActorName = "Sistem",        Title = "Durum değiştirildi",             Detail = "Açık → Atandı" },
            new() { Date = ticket.CreatedAt.AddHours(1),              Type = "assigned",    ActorName = "Selin Demir",   Title = "Personel atandı",                Detail = "Mehmet Kaya atandı" },
            new() { Date = ticket.CreatedAt.AddDays(1),               Type = "replied",     ActorName = "Mehmet Kaya",   Title = "Yanıt eklendi",                  Detail = "Yazıcı IP adresi kontrol edildi." },
            new() { Date = ticket.CreatedAt.AddDays(1).AddHours(3),   Type = "status",      ActorName = "Mehmet Kaya",   Title = "Durum değiştirildi",             Detail = "Atandı → İşlemde" },
            new() { Date = ticket.CreatedAt.AddDays(2),               Type = "replied",     ActorName = "Selin Demir",   Title = "Yanıt eklendi",                  Detail = "Statik IP ataması yapıldı." },
            new() { Date = ticket.CreatedAt.AddDays(2).AddHours(1),   Type = "note",        ActorName = "Mehmet Kaya",   Title = "İç not eklendi",                 Detail = "Sürücüler güncellendi, kullanıcı onayı bekleniyor." },
            new() { Date = ticket.UpdatedAt ?? ticket.CreatedAt.AddDays(3), Type = "status", ActorName = "Selin Demir",  Title = "Durum değiştirildi",             Detail = "İşlemde → Çözüldü" },
        };
        // ─────────────────────────────────────────────────────────────────────

        var model = new TicketDetailViewModel
        {
            Ticket      = ticket,
            Personnel   = personnel,
            Responses   = responses,
            Attachments = attachments,
            History     = history
        };
        return View(model);
    }

    // POST /Tickets/Ticket/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Assign(int ticketId, int assignedUserId, string? note)
    {
        TempData["SuccessMessage"] = "Personel atama yapıldı.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    // POST /Tickets/Ticket/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket ticket)
    {
        var response = await SendRequest<Ticket>("ticket/add", ticket, HttpMethod.Post);
        if (response?.Success == true)
            TempData["SuccessMessage"] = "Ticket başarıyla oluşturuldu.";
        else
            TempData["ErrorMessage"] = response?.Message ?? "Ticket oluşturulamadı.";

        return RedirectToAction(nameof(Index));
    }

    // POST /Tickets/Ticket/Reply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reply(int ticketId, string content)
    {
        TempData["SuccessMessage"] = "Yanıt gönderildi.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    // POST /Tickets/Ticket/Transfer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Transfer(int ticketId, int? targetUserId, string? note)
    {
        TempData["SuccessMessage"] = "Ticket sevk edildi.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    // POST /Tickets/Ticket/ChangeStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeStatus(int ticketId, int statusId)
    {
        TempData["SuccessMessage"] = "Durum güncellendi.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    // GET /Tickets/Ticket/GetCount — sidebar rozeti için
    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        var response = await SendRequest<List<Ticket>>("ticket/getall");
        var count = response?.Data?.Count ?? 4; // mock: API yoksa 4
        return Json(new { count });
    }

    // ── Mock Vekâlet Ticketları ────────────────────────────────────────────────
    // API hazır olduğunda bu metot kaldırılacak.
    private static List<Ticket> GetMockDelegatedTickets() => new()
    {
        new Ticket
        {
            Id = 101, TicketNo = "TKT-2026-101",
            Title       = "Sunucu Erişim Sorunu — IT Müdürlüğü",
            Description = "Merkez sunucuya VPN üzerinden erişilemiyor. 5 kullanıcı etkilendi.",
            StatusId    = 1, PriorityId = 4,
            CreatedById = 5, AssignedUserId = null, CurrentBranchId = 2,
            CreatedAt   = DateTime.Now.AddDays(-1)
        },
        new Ticket
        {
            Id = 102, TicketNo = "TKT-2026-102",
            Title       = "E-posta Gönderim Gecikmesi",
            Description = "Toplu e-postalar 2-3 saat gecikmeli iletiliyor.",
            StatusId    = 3, PriorityId = 2,
            CreatedById = 6, AssignedUserId = 7, CurrentBranchId = 2,
            CreatedAt   = DateTime.Now.AddDays(-2)
        },
        new Ticket
        {
            Id = 103, TicketNo = "TKT-2026-103",
            Title       = "Lisans Yenileme Talebi",
            Description = "Office 365 lisansları 15.03 tarihinde sona eriyor, yenileme yapılmalı.",
            StatusId    = 2, PriorityId = 3,
            CreatedById = 5, AssignedUserId = 8, CurrentBranchId = 2,
            CreatedAt   = DateTime.Now.AddDays(-4)
        },
        new Ticket
        {
            Id = 104, TicketNo = "TKT-2026-104",
            Title       = "Yeni Personel Bilgisayar Kurulumu",
            Description = "İşe başlayan 3 personel için laptop kurulumu ve hesap açılması gerekiyor.",
            StatusId    = 5, PriorityId = 1,
            CreatedById = 6, AssignedUserId = 7, CurrentBranchId = 2,
            CreatedAt   = DateTime.Now.AddDays(-6)
        },
    };
}
