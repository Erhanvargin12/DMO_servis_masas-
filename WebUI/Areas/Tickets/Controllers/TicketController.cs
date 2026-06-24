using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models;
using WebUI.Models.DTOs;

namespace WebUI.Areas.Talepler.Controllers;

[Area("Tickets")]
[Authorize(Roles = "Tickets,Staff")]
public class TicketController : BaseController
{
    private readonly IApiClient _apiClient;

    public TicketController(IApiClient apiClient) : base(apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? search, string? status, string? priority, string? tab)
    {
        tab ??= "own";

        var model = new TicketViewModel();
        var response = await SendRequest<List<Ticket>>("ticket/getall");

        model.Ticket = response;
        model.SearchTerm = search;
        model.StatusFilter = status;
        model.PriorityFilter = priority;
        model.ActiveTab = tab;

        // ── Mock vekalet bilgisi (API bağlanana kadar) ────────────────────────
        // API hazır olduğunda: kullanıcının aktif vekâletleri /delegation/getactive
        // endpoint'inden çekilecek ve bu blok kaldırılacak.
        var mockDelegation = new DelegationInfo
        {
            DelegatorId = 5,
            DelegatorName = "Ahmet Yılmaz",
            DelegationType = "manager",      // "manager" | "personnel"
            BranchName = "IT Müdürlüğü"
        };
        model.Delegation = mockDelegation;
        // ─────────────────────────────────────────────────────────────────────

        // Kendi ticketları
        var ownList = response?.Data ?? new List<Ticket>();

        // Vekâlet ticketları (mock) — API gelince /ticket/getdelegated/{delegatorId}
        var delegatedList = GetMockDelegatedTickets();

        // Sayaçlar (filtreden önce)
        model.OwnCount = ownList.Count;
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

    public async Task<IActionResult> PendingTickets(string? search, string? status, string? priority, string? tab)
    {
        tab ??= "own";

        var model = new TicketViewModel();
        var response = await SendRequest<List<Ticket>>("ticket/getall");

        model.Ticket = response;
        model.SearchTerm = search;
        model.StatusFilter = status;
        model.PriorityFilter = priority;
        model.ActiveTab = tab;

        // ── Mock vekalet bilgisi (API bağlanana kadar) ────────────────────────
        // API hazır olduğunda: kullanıcının aktif vekâletleri /delegation/getactive
        // endpoint'inden çekilecek ve bu blok kaldırılacak.
        var mockDelegation = new DelegationInfo
        {
            DelegatorId = 5,
            DelegatorName = "Ahmet Yılmaz",
            DelegationType = "manager",      // "manager" | "personnel"
            BranchName = "IT Müdürlüğü"
        };
        model.Delegation = mockDelegation;
        // ─────────────────────────────────────────────────────────────────────

        // Kendi ticketları
        var ownList = response?.Data ?? new List<Ticket>();

        // Vekâlet ticketları (mock) — API gelince /ticket/getdelegated/{delegatorId}
        var delegatedList = GetMockDelegatedTickets();

        // Sayaçlar (filtreden önce)
        model.OwnCount = ownList.Count;
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

    public async Task<IActionResult> Detail(int id)
    {
        var ticket = await SendRequest<TicketDto>($"ticket/getticketdtobyid/{id}");
        var branchStaff = await SendRequest<List<User>>("user/getall");
        var responses = await SendRequest<List<Response>>($"ticketresponse/getbyticketid/{id}");
        var attachments = await SendRequest<List<Attachment>>($"ticketattachment/getbyticketid/{id}");

        var model = new TicketDetailViewModel
        {
            TicketDto = ticket?.Data,
            Personnel = branchStaff?.Data?.Where(u => u.Status).ToList() ?? new List<User>(),
            Histories = GetTicketHistory(ticket.Data)
        };
        return View(model);
    }

    public List<TicketHistoryItem> GetTicketHistory(TicketDto dto)
    {
        var history = new List<TicketHistoryItem>();

        if (dto?.Ticket != null)
        {
            history.Add(new TicketHistoryItem
            {
                Date = dto.Ticket.CreatedAt,
                Type = "created",
                Title = "Talep Oluşturuldu",
                ActorName = "test@gmail.com",
                Detail = $"{dto.Ticket.TicketNo} numaralı talep test@gmail.com tarafından oluşturuldu"
            });

            if (dto.Ticket.UpdatedAt != null)
            {
                history.Add(new TicketHistoryItem
                {
                    Date = dto.Ticket.UpdatedAt,
                    Type = "status",
                    Title = "Durum Güncellendi",
                    ActorName = "test@gmail.com",
                    Detail = $"Durum: {dto.Ticket.StatusId}"
                });
            }
            
            if (dto.Ticket.ClosedAt != null)
            {
                history.Add(new TicketHistoryItem
                {
                    Date = dto.Ticket.ClosedAt.Value,
                    Type = "closed",
                    Title = "Talep Kapatıldı",
                    ActorName = "test@gmail.com",
                    Detail = $"{dto.Ticket.TicketNo} numaralı talep kapatıldı"
                });
            }
        }
        if (dto?.Assignment != null)
        {
            history.Add(new TicketHistoryItem
            {
                Date = dto.Assignment.AssignedAt,
                Type = "assigned",
                Title = "Talep Atandı",
                ActorName = "test@gmail.com",
                Detail = $"Talep {dto.Assignment.AssignedToId} kullanıcısına atandı"
            });

            if (dto.Assignment?.AssignedAt != null)
            {
                history.Add(new TicketHistoryItem
                {
                    Date = dto.Assignment.AssignedAt,
                    Type = "transferred",
                    Title = "Talep Aktarıldı",
                    ActorName = "test@gmail.com",
                    Detail = $"Talep başka kullanıcıya aktarıldı"
                });
            }
        }
        if (dto?.Responses != null)
        {
            history.AddRange(dto.Responses.Select(r => new TicketHistoryItem
            {
                Date = r.CreatedAt,
                Type = "replied",
                Title = r.IsInternal ? "Not Eklendi" : "Yanıt Verildi",
                ActorName = "test@gmail.com",
                Detail = "test"
            }));
        }

        return history.OrderBy(x => x.Date).ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int ticketId, int assignedUserId, string? note)
    {
        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);

        var assignment = new Assignment()
        {
            TicketId = ticketId,
            AssignedById = userId,
            AssignedToId = assignedUserId,
            AssignedAt = DateTime.Now,
            Description = note
        };
        var response = await SendRequest<Assignment>("ticketassignment/add", assignment, HttpMethod.Post);

        if (response?.Success == true)
            TempData["SuccessMessage"] = "Ticket başarıyla oluşturuldu.";
        else
            TempData["ErrorMessage"] = response?.Message ?? "Ticket oluşturulamadı.";

        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketDto ticketDto, List<IFormFile>? FormFiles)
    {
        ticketDto.Attachments = await SaveAttachmentFile(FormFiles);
        var response = await SendRequest<TicketDto>("ticket/add", ticketDto, HttpMethod.Post);

        if (response?.Success == true)
            TempData["SuccessMessage"] = "Ticket başarıyla oluşturuldu.";
        else
            TempData["ErrorMessage"] = response?.Message ?? "Ticket oluşturulamadı.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int ticketId, string content, List<IFormFile>? attachments)
    {
        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);

        var reply = new TicketResponseDto
        {
            Response = new Response
            {
                IsInternal = false,
                Message = content,
                CreatedAt = DateTime.Now,
                TicketId = ticketId,
                SenderId = userId
            },
            Attachments = await SaveAttachmentFile(attachments),

        };

        var response = await SendRequest<TicketResponseDto>($"ticketresponse/add",reply, HttpMethod.Post);

        TempData["SuccessMessage"] = "Yanıt gönderildi.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Transfer(int ticketId, int? targetUserId, string? note)
    {
        TempData["SuccessMessage"] = "Ticket sevk edildi.";
        return RedirectToAction(nameof(Detail), new { id = ticketId });
    }


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
    private async Task<List<Attachment>> SaveAttachmentFile(List<IFormFile>? FormFiles)
    {
        var attachments = new List<Attachment>();

        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);
        int.TryParse(HttpContext.Session.GetString("BranchId"), out var branchId);


        if (FormFiles != null && FormFiles.Count > 0)
        {
            
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
            Directory.CreateDirectory(uploadRoot);

            foreach (var file in FormFiles)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadRoot, uniqueName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                attachments.Add(new Attachment
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/tickets/{uniqueName}",
                    UploadedById = userId,
                    CreatedAt = DateTime.Now
                });
            }
        }

        return attachments.ToList();
    }
}
