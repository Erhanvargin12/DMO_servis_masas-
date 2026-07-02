using Core.Communication;
using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models;
using WebUI.Models.DTOs;
using WebUI.Services.Abstract;

namespace WebUI.Areas.Talepler.Controllers;

[Area("Tickets")]
[Authorize(Roles = "Tickets,Staff")]
public class TicketController : BaseController
{
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceCommunication _service;
    public TicketController(IApiClient apiClient,ICurrentUserService currentUser, IServiceCommunication service) : base(apiClient)
    {
        _currentUser = currentUser;
        _service = service;
    }

    public async Task<IActionResult> Index(string? search, string? status, string? priority, int? userId)
    {
        var model = new TicketViewModel
        {
            SearchTerm = search,
            StatusFilter = status,
            PriorityFilter = priority,
            ActiveUserId = userId
        };

        await VekilleriGetir(model);
        await KendiTaleplerimiGetir(model);

        var liste = await TalepleriGetir(userId);
        
        model.FilteredTickets = Filtrele(liste, search, status, priority);

        return View(model);
    }

    private async Task VekilleriGetir(TicketViewModel model)
    {
        if (_currentUser.User?.ProxyUserIds == null) return;

        foreach (var pId in _currentUser.User.ProxyUserIds)
        {
            var proxyUser = await _service.GetAsync<DataResult<User>>("dip", $"user/getbyid/{pId}");
            if (proxyUser?.Data != null)
            {
                model.ProxyUser.Add(proxyUser.Data);

                if (int.TryParse(pId, out int proxyUserId))
                {
                    var response = await _service.GetAsync<DataResult<List<Ticket>>>("dip", "ticket/get-ticket-by-user-id", new { id = proxyUserId });
                    model.ProxyCounts[proxyUserId] = response?.Data?.Count ?? 0;
                }
            }
        }
    }

    private async Task KendiTaleplerimiGetir(TicketViewModel model)
    {
        var response = await _service.GetAsync<DataResult<List<Ticket>>>("dip", "ticket/get-ticket-by-user-id", new { id = _currentUser.User.UserId });
        var liste = response?.Data ?? new List<Ticket>();
        
        model.OwnCount = liste.Count;
        model.Ticket = response;
    }

    private async Task<List<Ticket>> TalepleriGetir(int? userId)
    {
        int hedefId = userId ?? _currentUser.User.UserId;
        var response = await _service.GetAsync<DataResult<List<Ticket>>>("dip", "ticket/get-ticket-by-user-id", new { id = hedefId });
        return response?.Data ?? new List<Ticket>();
    }

    private List<Ticket> Filtrele(List<Ticket> liste, string? search, string? status, string? priority)
    {
        IEnumerable<Ticket> filtreli = liste;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            filtreli = filtreli.Where(t =>
                (!string.IsNullOrEmpty(t.Title) && t.Title.ToLowerInvariant().Contains(s)) ||
                (!string.IsNullOrEmpty(t.TicketNo) && t.TicketNo.ToLowerInvariant().Contains(s)) ||
                (!string.IsNullOrEmpty(t.Description) && t.Description.ToLowerInvariant().Contains(s))
            );
        }

        if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out var sid))
        {
            filtreli = filtreli.Where(t => t.StatusId == sid);
        }

        if (!string.IsNullOrWhiteSpace(priority) && int.TryParse(priority, out var pid))
        {
            filtreli = filtreli.Where(t => t.PriorityId == pid);
        }

        return filtreli.ToList();
    }

    public async Task<IActionResult> PendingTickets(string? search, string? status, string? priority, int? userId)
    {
        var model = new TicketViewModel
        {
            SearchTerm = search,
            StatusFilter = status,
            PriorityFilter = priority,
            ActiveUserId = userId
        };

        await VekilleriGetir(model);
        await KendiTaleplerimiGetir(model);

        var liste = await TalepleriGetir(userId);
        
        model.FilteredTickets = Filtrele(liste, search, status, priority);

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
        ticketDto.Ticket.CreatedById = _currentUser.User.UserId;
        
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
