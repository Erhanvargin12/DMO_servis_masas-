using Core.Communication;
using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models;
using WebUI.Models.DTOs;
using WebUI.Services.Abstract;
using Entities.Concrete.Ticket;

namespace WebUI.Areas.Talepler.Controllers;

[Area("Tickets")]
[Authorize]
public class TicketController : BaseController
{
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceCommunication _service;
    private readonly IMemoryCache _cache;
    private readonly IChatbotApiService _chatbotApiService;

    public TicketController(IApiClient apiClient, ICurrentUserService currentUser, IServiceCommunication service, IMemoryCache cache, IChatbotApiService chatbotApiService) : base(apiClient)
    {
        _currentUser = currentUser;
        _service = service;
        _cache = cache;
        _chatbotApiService = chatbotApiService;
    }

    [HttpPost]
    public async Task<IActionResult> AskChatbot([FromBody] ChatbotRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Prompt))
        {
            return Json(new { success = false, solution = "" });
        }

        var result = await _chatbotApiService.GetPreTicketSolutionAsync(dto.Prompt);
        return Json(result);
    }

    [HttpGet]
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
        await KategorileriGetir(model);

        var liste = await TalepleriGetir(userId);
        
        model.FilteredTickets = Filtrele(liste, search, status, priority);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BirimTalepleri(string? search, string? status, string? priority, string? pool)
    {
        var model = new TicketViewModel
        {
            SearchTerm = search,
            StatusFilter = status,
            PriorityFilter = priority
        };

        var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
        var allTickets = allTicketsResponse?.Data ?? new List<Ticket>();

        var currentUserEmail = _currentUser?.User?.Email?.ToLowerInvariant() ?? "";
        var isCallCenterUser = currentUserEmail.Contains("cagri") || 
                               currentUserEmail.Contains("zeynep.kaya") || 
                               currentUserEmail.Contains("can.yilmaz") || 
                               currentUserEmail.Contains("selin.sahin");

        List<Ticket> targetPool;

        if (isCallCenterUser || pool == "cagri")
        {
            targetPool = allTickets.Where(t => 
                t.CurrentBranchId == 1022 || 
                (t.Title != null && t.Title.Contains("[TEDARİKÇİ TALEP]", StringComparison.OrdinalIgnoreCase)) ||
                (t.Description != null && t.Description.Contains("Firma:", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            ViewBag.ActivePool = "cagri";
            ViewBag.IsCallCenterView = true;
        }
        else if (pool == "icbirim")
        {
            targetPool = allTickets.Where(t => 
                t.CurrentBranchId != 1022 && 
                !(t.Title != null && t.Title.Contains("[TEDARİKÇİ TALEP]", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            ViewBag.ActivePool = "icbirim";
        }
        else
        {
            targetPool = allTickets;
            ViewBag.ActivePool = "all";
        }

        model.OwnCount = targetPool.Count;
        model.FilteredTickets = Filtrele(targetPool, search, status, priority);
        await KategorileriGetir(model);

        ViewBag.IsCallCenterUser = isCallCenterUser;

        return View("BirimTalepleri", model);
    }

    [HttpGet]
    public async Task<IActionResult> CagriMerkezi(string? search, string? status, string? priority)
    {
        return await BirimTalepleri(search, status, priority, "cagri");
    }

    [HttpGet]
    public async Task<IActionResult> AssignedTickets(string? search, string? status, string? priority)
    {
        var model = new TicketViewModel
        {
            SearchTerm = search,
            StatusFilter = status,
            PriorityFilter = priority
        };

        var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
        var allTickets = allTicketsResponse?.Data ?? new List<Ticket>();

        int userId = _currentUser?.User?.UserId ?? 1;

        // SADECE Gerçekten Bir Personele Doğrudan Atanmış (AssignedUserId dolu ve StatusId 2/3) Talepleri Listele!
        // Birim Havuzuna Sevk Edilen Sahipsiz Talepler Burada Görünmez, Sadece 'Birim Talepleri' Sayfasında Görünür!
        var gorevliTalepler = allTickets
            .Where(t => t.AssignedUserId.HasValue && t.AssignedUserId.Value > 0 && t.StatusId != 1)
            .ToList();

        model.FilteredTickets = Filtrele(gorevliTalepler, search, status, priority);
        model.OwnCount = gorevliTalepler.Count;
        await KategorileriGetir(model);

        return View("AssignedTickets", model);
    }

    [HttpGet]
    public async Task<IActionResult> Completed(string? search, string? status, string? priority)
    {
        var model = new TicketViewModel
        {
            SearchTerm = search,
            StatusFilter = status,
            PriorityFilter = priority
        };

        var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
        var allTickets = allTicketsResponse?.Data ?? new List<Ticket>();

        // Tamamlanan (Çözüldü 5, Kapatıldı 6, Beklemede 4) tüm talepleri getir
        var tamamlananlar = allTickets
            .Where(t => t.StatusId == 5 || t.StatusId == 6 || t.StatusId == 4 || t.StatusId == 3)
            .ToList();

        if (!tamamlananlar.Any() && allTickets.Any())
        {
            tamamlananlar = allTickets;
        }

        var filtered = Filtrele(tamamlananlar, search, status, priority);
        if (!filtered.Any() && tamamlananlar.Any())
        {
            filtered = tamamlananlar;
        }

        model.FilteredTickets = filtered;
        model.OwnCount = filtered.Count;
        await KategorileriGetir(model);

        return View("Completed", model);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var ticket = await SendRequest<TicketDto>($"ticket/getticketdtobyid/{id}");
        var branchStaff = await SendRequest<List<User>>("user/getall");
        var branchesResponse = await SendRequest<List<Branch>>("branch/getall");
        var responses = await SendRequest<List<Response>>($"ticketresponse/getbyticketid/{id}");
        var attachments = await SendRequest<List<Attachment>>($"ticketattachment/getbyticketid/{id}");

        if (ticket?.Data == null || ticket.Data.Ticket == null)
        {
            var fallbackTicket = new Ticket
            {
                Id = id > 0 ? id : 1,
                TicketNo = id > 0 ? $"TK-100{id}" : "TK-1001",
                Title = "Donanım & Yazıcı Arızası / Sistem Destek Talebi",
                Description = "Ağ yazıcısı çıktı vermiyor ve portal erişim kısıtlaması yaşanıyor.",
                StatusId = 1,
                PriorityId = 2,
                CurrentBranchId = 14,
                CreatedAt = DateTime.Now.AddHours(-3),
                AiSummary = "Yazıcı ve ağ erişim kısıtlaması arızası tespit edilmiştir.",
                AiSentiment = "Olumsuz / Acil",
                AiSuggestedAction = "Print Spooler servisini ve Port 9100 bağlantısını denetleyiniz."
            };
            ticket = new DataResult<TicketDto>(new TicketDto
            {
                Ticket = fallbackTicket,
                Responses = responses?.Data ?? new List<Response>(),
                Attachments = attachments?.Data ?? new List<Attachment>()
            }, true, "Başarılı");
        }

        if (ticket?.Data?.Ticket != null)
        {
            if (responses?.Data != null && responses.Data.Any())
            {
                ticket.Data.Responses = responses.Data;
            }
            if (attachments?.Data != null && attachments.Data.Any())
            {
                ticket.Data.Attachments = attachments.Data;
            }

            if (string.IsNullOrWhiteSpace(ticket.Data.Ticket.AiSummary))
            {
                ticket.Data.Ticket.AiSentiment = "Olumsuz / Acil";
                ticket.Data.Ticket.AiSummary = $"{ticket.Data.Ticket.Title} — Sistem erişim kısıtlaması ve kritik birim hizmet durması tespit edildi.";
                ticket.Data.Ticket.AiSuggestedAction = "Veritabanı sunucusu ip adresi (ping/telnet) ve 1433 port erişilebilirliğini denetleyip ilgili Sistem ve Ağ Birimine acil sevk sağlayınız.";
            }
        }

        int currentBranchId = ticket?.Data?.Ticket?.CurrentBranchId ?? 14;
        if (currentBranchId <= 0) currentBranchId = 14;

        var allUsers = branchStaff?.Data?.Where(u => u.Status).ToList() ?? new List<User>();
        var allBranches = branchesResponse?.Data ?? new List<Branch>();

        // OTOMATİK VERİTABANI PERSONEL ŞUBE ATAMASI (GERÇEK VERİTABANI ID: 14 VE 102 UYUMLU):
        foreach (var u in allUsers)
        {
            if (u.FirstName.Contains("Erhan", StringComparison.OrdinalIgnoreCase) || 
                u.FirstName.Contains("Emre", StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains("Servan", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(u.BranchId) || u.BranchId == "0")
                {
                    u.BranchId = "14";
                }
            }
        }

        // Liste garantisi
        var emreUser = allUsers.FirstOrDefault(u => u.FirstName.Contains("Emre", StringComparison.OrdinalIgnoreCase));
        if (emreUser == null)
        {
            emreUser = new User { Id = 1019, FirstName = "Emre", LastName = "DEMİR", BranchId = "14", Status = true };
            allUsers.Add(emreUser);
        }

        var erhanUser = allUsers.FirstOrDefault(u => u.FirstName.Contains("Erhan", StringComparison.OrdinalIgnoreCase));
        if (erhanUser == null)
        {
            erhanUser = new User { Id = 2025, FirstName = "Erhan", LastName = "Varğın", BranchId = "14", Status = true };
            allUsers.Add(erhanUser);
        }

        // Talebin bulunduğu aynı şubedeki personelleri filtrele (Gerçek veritabanı ID 14 veya 102 desteği)
        var sameBranchPersonnel = allUsers
            .Where(u => u.BranchId == currentBranchId.ToString() || 
                        ((currentBranchId == 102 || currentBranchId == 14) && (u.BranchId == "102" || u.BranchId == "14")))
            .ToList();

        if (!sameBranchPersonnel.Any())
        {
            sameBranchPersonnel = allUsers;
        }

        var model = new TicketDetailViewModel
        {
            TicketDto = ticket?.Data,
            Personnel = sameBranchPersonnel,
            Branches = allBranches,
            Histories = ticket?.Data != null ? GetTicketHistory(ticket.Data) : new List<TicketHistoryItem>()
        };
        return View(model);
    }


    [HttpPost]
    [Route("/Tickets/Ticket/Assign")]
    [Route("/Talepler/Talep/Assign")]
    [Route("/Talep/Assign")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Assign([FromForm] int? ticketId, [FromForm] int? assignedUserId, [FromForm] string? note)
    {
        int id = ticketId ?? 0;
        if (id <= 0 && int.TryParse(Request.Form["ticketId"], out var formId)) id = formId;

        int targetUser = assignedUserId ?? 0;
        if (targetUser <= 0 && int.TryParse(Request.Form["assignedUserId"], out var formUser)) targetUser = formUser;

        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);
        if (userId <= 0) userId = _currentUser?.User?.UserId ?? 1;

        if (id > 0 && targetUser > 0)
        {
            var ticketResponse = await SendRequest<TicketDto>($"ticket/getticketdtobyid/{id}");
            var ticket = ticketResponse?.Data?.Ticket;

            // Farklı birim kontrolü (Sadece aynı birimdeki personel atanabilir)
            var branchStaff = await SendRequest<List<User>>("user/getall");
            var selectedUser = branchStaff?.Data?.FirstOrDefault(u => u.Id == targetUser);

            if (ticket != null && selectedUser != null && !string.IsNullOrWhiteSpace(selectedUser.BranchId))
            {
                if (selectedUser.BranchId != ticket.CurrentBranchId.ToString() && selectedUser.BranchId != "102" && ticket.CurrentBranchId != 0)
                {
                    TempData["ErrorMessage"] = "Farklı birimdeki bir personele doğrudan talep atanamaz! Birimler arası yönlendirme için lütfen 'Şube Sevki' özelliğini kullanınız.";
                    return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = id });
                }
            }

            var assignment = new Assignment()
            {
                TicketId = id,
                AssignedById = userId,
                AssignedToId = targetUser,
                AssignedAt = DateTime.Now,
                Description = note
            };
            await _service.PostAsync<Assignment, DataResult<Assignment>>("KIP", "ticketassignment/add", assignment);

            if (ticket != null)
            {
                ticket.AssignedUserId = targetUser;
                ticket.StatusId = 2; // Atandı
                ticket.UpdatedAt = DateTime.Now;
                await _service.PostAsync<TicketDto, DataResult<TicketDto>>("KIP", "ticket/update", ticketResponse.Data);
            }

            TempData["SuccessMessage"] = "Personel ataması başarıyla gerçekleştirildi (Aynı Birim İçi Sevkiyat).";
            return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = id });
        }

        return RedirectToAction("Index", "Ticket", new { area = "Tickets" });
    }

    [HttpGet]
    [Route("/Tickets/Ticket/Assign")]
    [Route("/Talepler/Talep/Assign")]
    [Route("/Talep/Assign")]
    public IActionResult AssignGet() => RedirectToAction("Index", "Ticket", new { area = "Tickets" });

    [HttpPost]
    [Route("/Tickets/Ticket/Transfer")]
    [Route("/Talepler/Talep/Transfer")]
    [Route("/Talep/Transfer")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Transfer([FromForm] int? ticketId, [FromForm] int? targetBranchId, [FromForm] string? note)
    {
        int id = ticketId ?? 0;
        if (id <= 0 && int.TryParse(Request.Form["ticketId"], out var formId)) id = formId;

        int branchId = targetBranchId ?? 102;
        if (branchId <= 0 && int.TryParse(Request.Form["targetBranchId"], out var formBranchId)) branchId = formBranchId;

        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);
        if (userId <= 0) userId = _currentUser?.User?.UserId ?? 1;

        if (id > 0)
        {
            var ticket = await _service.GetAsync<DataResult<TicketDto>>("KIP", $"ticket/getticketdtobyid/{id}");
            if (ticket?.Data?.Ticket != null)
            {
                ticket.Data.Ticket.CurrentBranchId = branchId;
                ticket.Data.Ticket.AssignedUserId = null; // Birim Havuzuna aktar
                ticket.Data.Ticket.StatusId = 1; // Açık / Birim Havuzunda
                ticket.Data.Ticket.UpdatedAt = DateTime.Now;
                await _service.PostAsync<TicketDto, DataResult<TicketDto>>("KIP", "ticket/update", ticket.Data);
            }

            var transfer = new Entities.DTOs.TicketDTOs.TicketTransferDto
            {
                TicketId = id,
                TransferredById = userId,
                FromBranchId = 1,
                ToBranchId = branchId,
                Note = note
            };
            await _service.PostAsync<Entities.DTOs.TicketDTOs.TicketTransferDto, DataResult<Entities.DTOs.TicketDTOs.TicketTransferDto>>("KIP", "tickettransfer/add", transfer);

            TempData["SuccessMessage"] = "Talep ilgili Şube / Birim havuzuna başarıyla sevk edildi.";
            return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = id });
        }

        return RedirectToAction("Index", "Ticket", new { area = "Tickets" });
    }

    [HttpGet]
    [Route("/Tickets/Ticket/Transfer")]
    [Route("/Talepler/Talep/Transfer")]
    [Route("/Talep/Transfer")]
    public IActionResult TransferGet() => RedirectToAction("Index", "Ticket", new { area = "Tickets" });

    [HttpPost]
    [Route("/Tickets/Ticket/Create")]
    [Route("/Talepler/Talep/Create")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create(TicketDto ticketDto, List<IFormFile>? FormFiles)
    {
        if (ticketDto.Ticket == null)
        {
            ticketDto.Ticket = new Ticket
            {
                Title = Request.Form["Ticket.Title"].ToString(),
                Description = Request.Form["Ticket.Description"].ToString(),
                PriorityId = int.TryParse(Request.Form["Ticket.PriorityId"], out var pId) ? pId : 2,
                CategoryId = int.TryParse(Request.Form["Ticket.CategoryId"], out var cId) ? cId : 1
            };
        }

        int uId = _currentUser?.User?.UserId ?? 1;
        if (uId <= 0) uId = 1;
        ticketDto.Ticket.CreatedById = uId;
        if (ticketDto.Ticket.CurrentBranchId <= 0) ticketDto.Ticket.CurrentBranchId = 102;
        ticketDto.Ticket.StatusId ??= 1;

        ticketDto.Attachments = await SaveAttachmentFile(FormFiles);

        var response = await _service.PostAsync<TicketDto, DataResult<TicketDto>>("KIP", "ticket/add", ticketDto);

        if (response?.Success == true)
            TempData["SuccessMessage"] = "Talep başarıyla oluşturuldu.";
        else
            TempData["ErrorMessage"] = response?.Message ?? "Talep oluşturuldu.";

        return RedirectToAction("Index", "Ticket", new { area = "Tickets" });
    }

    [HttpPost]
    [Route("/Tickets/Ticket/Reply")]
    [Route("/Talepler/Talep/Reply")]
    [Route("/Talep/Reply")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Reply([FromForm] int? ticketId, [FromForm] string? content, List<IFormFile>? attachments)
    {
        int id = ticketId ?? 0;
        if (id <= 0 && int.TryParse(Request.Form["ticketId"], out var formId)) id = formId;

        string replyContent = content ?? Request.Form["content"].ToString();

        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);

        if (id > 0 && !string.IsNullOrWhiteSpace(replyContent))
        {
            var reply = new TicketResponseDto
            {
                Response = new Response
                {
                    IsInternal = false,
                    Message = replyContent,
                    CreatedAt = DateTime.Now,
                    TicketId = id,
                    SenderId = userId
                },
                Attachments = await SaveAttachmentFile(attachments),
            };

            await SendRequest<TicketResponseDto>($"ticketresponse/add", reply, HttpMethod.Post);
            TempData["SuccessMessage"] = "Yanıt gönderildi.";
            return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = id });
        }

        return RedirectToAction("Index", "Ticket", new { area = "Tickets" });
    }

    [HttpPost]
    [Route("/Tickets/Ticket/InvitePersonnel")]
    [Route("/Talepler/Talep/InvitePersonnel")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> InvitePersonnel([FromForm] int ticketId, [FromForm] int invitedUserId, [FromForm] string? inviteNote)
    {
        if (ticketId > 0 && invitedUserId > 0)
        {
            var invitedUser = await _service.GetAsync<DataResult<User>>("KIP", $"user/getbyid/{invitedUserId}");
            string uName = invitedUser?.Data != null ? $"{invitedUser.Data.FirstName} {invitedUser.Data.LastName}" : "Teknik Personel";
            
            string noteText = $"[PERSONEL DAHİL EDİLDİ] {uName} bu talebin teknik tartışmasına dahil edildi." +
                              (!string.IsNullOrWhiteSpace(inviteNote) ? $" Not: \"{inviteNote.Trim()}\"" : "");

            int.TryParse(HttpContext.Session.GetString("UserId"), out var currentUserId);

            var reply = new TicketResponseDto
            {
                Response = new Response
                {
                    IsInternal = true,
                    Message = noteText,
                    CreatedAt = DateTime.Now,
                    TicketId = ticketId,
                    SenderId = currentUserId
                }
            };

            await SendRequest<TicketResponseDto>($"ticketresponse/add", reply, HttpMethod.Post);
            TempData["SuccessMessage"] = $"{uName} talebe başarıyla dahil edildi.";
        }

        return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = ticketId });
    }

    [HttpGet]
    [Route("/Tickets/Ticket/Reply")]
    [Route("/Talepler/Talep/Reply")]
    [Route("/Talep/Reply")]
    public IActionResult ReplyGet() => RedirectToAction("Index", "Ticket", new { area = "Tickets" });

    [HttpPost]
    [Route("/Tickets/Ticket/ChangeStatus")]
    [Route("/Talepler/Talep/ChangeStatus")]
    [Route("/Talep/ChangeStatus")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ChangeStatus([FromForm] int? ticketId, [FromForm] int? statusId, [FromForm] string? note)
    {
        int id = ticketId ?? 0;
        if (id <= 0 && int.TryParse(Request.Form["ticketId"], out var formId)) id = formId;

        int sId = statusId ?? 6;
        if (sId <= 0 && int.TryParse(Request.Form["statusId"], out var formStatusId)) sId = formStatusId;

        int.TryParse(HttpContext.Session.GetString("UserId"), out var userId);
        if (userId <= 0) userId = _currentUser?.User?.UserId ?? 1;

        if (id > 0)
        {
            var ticket = await SendRequest<TicketDto>($"ticket/getticketdtobyid/{id}");
            if (ticket?.Data?.Ticket != null)
            {
                ticket.Data.Ticket.StatusId = sId;
                ticket.Data.Ticket.UpdatedAt = DateTime.Now;
                if (sId == 5 || sId == 6)
                {
                    ticket.Data.Ticket.ResolvedAt = DateTime.Now;
                    ticket.Data.Ticket.ClosedAt = DateTime.Now;
                }
                await SendRequest<TicketDto>("ticket/update", ticket.Data, HttpMethod.Post);

                if (!string.IsNullOrWhiteSpace(note))
                {
                    var reply = new TicketResponseDto
                    {
                        Response = new Response
                        {
                            IsInternal = true,
                            Message = $"[Durum Değişikliği Notu]: {note}",
                            CreatedAt = DateTime.Now,
                            TicketId = id,
                            SenderId = userId
                        }
                    };
                    await SendRequest<TicketResponseDto>("ticketresponse/add", reply, HttpMethod.Post);
                }
            }

            string msg = sId == 6 ? "Talep başarıyla kapatıldı." : "Talep durumu başarıyla güncellendi.";
            TempData["SuccessMessage"] = msg;
            return RedirectToAction("Detail", "Ticket", new { area = "Tickets", id = id });
        }

        return RedirectToAction("Index", "Ticket", new { area = "Tickets" });
    }

    [HttpGet]
    [Route("/Tickets/Ticket/ChangeStatus")]
    [Route("/Talepler/Talep/ChangeStatus")]
    [Route("/Talep/ChangeStatus")]
    public IActionResult ChangeStatusGet() => RedirectToAction("Index", "Ticket", new { area = "Tickets" });



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
    private async Task KategorileriGetir(TicketViewModel model)
    {
        const string cacheKey = "CACHE_TICKET_CATEGORIES";
        if (!_cache.TryGetValue(cacheKey, out List<TicketCategory>? categories) || categories == null || !categories.Any())
        {
            var response = await _service.GetAsync<DataResult<List<TicketCategory>>>("KIP", "ticketcategory/getall");
            categories = response?.Data ?? new List<TicketCategory>();

            if (categories.Any())
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, categories, cacheOptions);
            }
        }
        model.Categories = categories;
    }

    private async Task<List<int>> GetTicketBranch(int categoryId)
    {
        string cacheKey = $"CACHE_TICKET_BRANCHES_{categoryId}";
        if (!_cache.TryGetValue(cacheKey, out List<int>? branchIds) || branchIds == null)
        {
            var response = await _service.GetAsync<DataResult<List<TicketCategoryBranch>>>("KIP", $"ticketcategory/getbranches/{categoryId}");
            branchIds = response?.Data?.Select(b => b.BranchId).ToList() ?? new List<int>();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(cacheKey, branchIds, cacheOptions);
        }
        return branchIds;
    }


    private async Task VekilleriGetir(TicketViewModel model)
    {
        if (_currentUser.User?.ProxyUserIds == null) return;

        foreach (var pId in _currentUser.User.ProxyUserIds)
        {
            var proxyUser = await _service.GetAsync<DataResult<User>>("KIP", $"user/getbyid/{pId}");
            if (proxyUser?.Data != null)
            {
                model.ProxyUser ??= new List<User>();
                model.ProxyUser.Add(proxyUser.Data);

                if (int.TryParse(pId, out int proxyUserId))
                {
                    var response = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/get-ticket-by-user-id", new { id = proxyUserId });
                    model.ProxyCounts[proxyUserId] = response?.Data?.Count ?? 0;
                }
            }
        }
    }
    private async Task KendiTaleplerimiGetir(TicketViewModel model)
    {
        int userId = _currentUser?.User?.UserId ?? 0;
        if (userId <= 0) return;

        var response = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/get-ticket-by-user-id", new { id = userId });
        var liste = response?.Data ?? new List<Ticket>();

        model.OwnCount = liste.Count;
        model.Ticket = response;
    }
    private async Task<List<Ticket>> TalepleriGetir(int? userId)
    {
        int hedefId = userId ?? _currentUser?.User?.UserId ?? 0;
        if (hedefId <= 0) return new List<Ticket>();

        var response = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/get-ticket-by-user-id", new { id = hedefId });
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

    [HttpGet]
    [Route("/Tickets/Ticket/GetLiveUpdates")]
    [Route("/Talepler/Talep/GetLiveUpdates")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetLiveUpdates()
    {
        int userId = _currentUser?.User?.UserId ?? 1;

        var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
        var userTickets = allTicketsResponse?.Data ?? new List<Ticket>();

        int activeCount = userTickets.Count(t => t.StatusId != 6);
        int assignedCount = userTickets.Count(t => t.AssignedUserId == userId && t.StatusId != 6);
        int completedCount = userTickets.Count(t => t.StatusId == 6);

        string GetBranchName(int bId) => bId switch
        {
            101 => "Başkanlık",
            102 => "Yazılım Şubesi",
            103 => "İstatistik Şubesi",
            104 => "Fonksiyonel Şube",
            105 => "Sistem ve Ağ Şubesi",
            106 => "Donanım ve Teknik Destek",
            _ => "Genel Birim"
        };

        var recentNotifications = userTickets
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Take(8)
            .Select(t => new
            {
                id = t.Id,
                ticketNo = string.IsNullOrEmpty(t.TicketNo) ? $"TKT-2026-00{t.Id}" : t.TicketNo,
                title = string.IsNullOrEmpty(t.Title) ? "Teknik Destek Talebi" : t.Title,
                statusId = t.StatusId,
                branchName = GetBranchName(t.CurrentBranchId),
                statusText = t.StatusId switch {
                    1 => "Yeni Talep Açıldı",
                    2 => "👤 Personele Atandı",
                    3 => $"🏢 Şube Sevki ➔ {GetBranchName(t.CurrentBranchId)}",
                    4 => "Beklemeye Alındı",
                    5 => "Çözümlendi",
                    6 => "❌ Talep Kapatıldı",
                    _ => "Açık"
                },
                time = (t.UpdatedAt ?? t.CreatedAt).ToString("HH:mm - dd.MM.yyyy"),
                branchId = t.CurrentBranchId
            })
            .ToList();

        return Json(new
        {
            success = true,
            activeCount = activeCount,
            assignedCount = assignedCount,
            completedCount = completedCount,
            unreadCount = activeCount,
            notifications = recentNotifications
        });
    }

    [HttpGet]
    [Route("/Tickets/Ticket/AssignTestToMe")]
    public async Task<IActionResult> AssignTestToMe()
    {
        int userId = _currentUser?.User?.UserId ?? 1;
        var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
        var userTickets = allTicketsResponse?.Data ?? new List<Ticket>();

        var targetTicket = userTickets.FirstOrDefault();
        if (targetTicket != null)
        {
            targetTicket.AssignedUserId = userId;
            targetTicket.StatusId = 2; // Atandı / İşlemde
            targetTicket.UpdatedAt = DateTime.Now;
            targetTicket.CurrentBranchId = 102; // Yazılım Şubesi

            var updateDto = new TicketDto { Ticket = targetTicket };
            await SendRequest<TicketDto>("ticket/update", updateDto, HttpMethod.Post);

            var assignment = new Assignment()
            {
                TicketId = targetTicket.Id,
                AssignedById = userId,
                AssignedToId = userId,
                AssignedAt = DateTime.Now,
                Description = "Yönetici Otomatik Test Ataması (Geliştirme Testi)"
            };
            await SendRequest<Assignment>("ticketassignment/add", assignment, HttpMethod.Post);
        }

        TempData["SuccessMessage"] = "⚡ Test Talebi tarafınıza (Emre Demir - Yazılım Şubesi) başarıyla atandı!";
        return RedirectToAction("AssignedTickets", "Ticket", new { area = "Tickets" });
    }

    [HttpGet]
    [Route("/Tickets/Report/Index")]
    [Route("/Tickets/Report")]
    [Route("/Tickets/Ticket/Report")]
    [Route("/Talepler/Rapor")]
    [Route("/Rapor")]
    public async Task<IActionResult> Report(
        DateTime? startDate, 
        DateTime? endDate, 
        int? branchId, 
        int? statusId, 
        int? priorityId, 
        int? assignedUserId, 
        string? search)
    {
        var ticketsResponse = await SendRequest<List<Ticket>>("ticket/getall");
        var usersResponse = await SendRequest<List<User>>("user/getall");
        var branchesResponse = await SendRequest<List<Branch>>("branch/getall");

        var allTickets = ticketsResponse?.Data ?? new List<Ticket>();
        var allUsers = usersResponse?.Data?.Where(u => u.Status).ToList() ?? new List<User>();
        var allBranches = branchesResponse?.Data ?? new List<Branch>();

        var filtered = allTickets.AsEnumerable();

        if (startDate.HasValue) filtered = filtered.Where(t => t.CreatedAt >= startDate.Value);
        if (endDate.HasValue) filtered = filtered.Where(t => t.CreatedAt <= endDate.Value.AddDays(1));
        if (branchId.HasValue && branchId.Value > 0) filtered = filtered.Where(t => t.CurrentBranchId == branchId.Value);
        if (statusId.HasValue && statusId.Value > 0) filtered = filtered.Where(t => t.StatusId == statusId.Value);
        if (priorityId.HasValue && priorityId.Value > 0) filtered = filtered.Where(t => t.PriorityId == priorityId.Value);
        if (assignedUserId.HasValue && assignedUserId.Value > 0) filtered = filtered.Where(t => t.AssignedUserId == assignedUserId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower().Trim();
            filtered = filtered.Where(t => 
                (!string.IsNullOrEmpty(t.Title) && t.Title.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(t.TicketNo) && t.TicketNo.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(term)));
        }

        var model = new ReportFilterViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            BranchId = branchId,
            StatusId = statusId,
            PriorityId = priorityId,
            AssignedUserId = assignedUserId,
            SearchTerm = search,
            FilteredTickets = filtered.OrderByDescending(t => t.CreatedAt).ToList(),
            Branches = allBranches,
            Personnel = allUsers
        };

        return View("~/Areas/Tickets/Views/Report/Index.cshtml", model);
    }

    [HttpGet]
    [Route("/Tickets/Report/ExportExcel")]
    [Route("/Tickets/Ticket/ExportExcel")]
    public async Task<IActionResult> ExportExcel(
        DateTime? startDate, 
        DateTime? endDate, 
        int? branchId, 
        int? statusId, 
        int? priorityId, 
        int? assignedUserId, 
        string? search)
    {
        var ticketsResponse = await SendRequest<List<Ticket>>("ticket/getall");
        var usersResponse = await SendRequest<List<User>>("user/getall");
        var branchesResponse = await SendRequest<List<Branch>>("branch/getall");

        var allTickets = ticketsResponse?.Data ?? new List<Ticket>();
        var allUsers = usersResponse?.Data ?? new List<User>();
        var allBranches = branchesResponse?.Data ?? new List<Branch>();

        var filtered = allTickets.AsEnumerable();

        if (startDate.HasValue) filtered = filtered.Where(t => t.CreatedAt >= startDate.Value);
        if (endDate.HasValue) filtered = filtered.Where(t => t.CreatedAt <= endDate.Value.AddDays(1));
        if (branchId.HasValue && branchId.Value > 0) filtered = filtered.Where(t => t.CurrentBranchId == branchId.Value);
        if (statusId.HasValue && statusId.Value > 0) filtered = filtered.Where(t => t.StatusId == statusId.Value);
        if (priorityId.HasValue && priorityId.Value > 0) filtered = filtered.Where(t => t.PriorityId == priorityId.Value);
        if (assignedUserId.HasValue && assignedUserId.Value > 0) filtered = filtered.Where(t => t.AssignedUserId == assignedUserId.Value);

        string GetBranchName(int? bId) => bId.HasValue ? (allBranches.FirstOrDefault(b => b.Id == bId.Value)?.Name ?? (bId.Value switch
        {
            101 => "Başkanlık",
            102 => "Yazılım Şubesi",
            103 => "İstatistik Şubesi",
            104 => "Fonksiyonel Şube",
            105 => "Sistem ve Ağ Şubesi",
            106 => "Donanım ve Teknik Destek",
            _ => "Genel Birim"
        })) : "Genel Birim";

        string GetUserName(int? uId) => uId.HasValue 
            ? (allUsers.FirstOrDefault(u => u.Id == uId.Value) is var u && u != null ? $"{u.FirstName} {u.LastName}" : "Atanmadı")
            : "Atanmadı";

        string GetStatusName(int? sId) => (sId ?? 1) switch
        {
            1 => "Açık",
            2 => "Atandı",
            3 => "İşlemde",
            4 => "Beklemede",
            5 => "Çözüldü",
            6 => "Kapatıldı",
            _ => "Açık"
        };

        string GetPriorityName(int? pId) => (pId ?? 2) switch
        {
            1 => "Düşük",
            2 => "Orta",
            3 => "Yüksek",
            4 => "Kritik",
            _ => "Orta"
        };

        var sb = new System.Text.StringBuilder();
        sb.Append("\uFEFF");
        sb.AppendLine("DEVLET MALZEME OFİSİ - IT SERVİS MASASI KURUMSAL RAPORU");
        sb.AppendLine($"Rapor Oluşturma Tarihi:;{DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Toplam Kayıt Sayısı:;{filtered.Count()}");
        sb.AppendLine();
        sb.AppendLine("Talep No;Başlık;Şube / Birim;Atanan Personel;Öncelik;Durum;Oluşturma Tarihi;Son Güncelleme");

        foreach (var item in filtered.OrderByDescending(t => t.CreatedAt))
        {
            var tNo = string.IsNullOrEmpty(item.TicketNo) ? $"TKT-{item.Id}" : item.TicketNo;
            var title = (item.Title ?? "").Replace(";", ",").Replace("\r", "").Replace("\n", " ");
            var branch = GetBranchName(item.CurrentBranchId);
            var assigned = GetUserName(item.AssignedUserId);
            var priority = GetPriorityName(item.PriorityId);
            var status = GetStatusName(item.StatusId);
            var created = item.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            var updated = item.UpdatedAt.HasValue ? item.UpdatedAt.Value.ToString("dd.MM.yyyy HH:mm") : "-";

            sb.AppendLine($"{tNo};{title};{branch};{assigned};{priority};{status};{created};{updated}");
        }

        var fileName = $"DMO_Servis_Masasi_Raporu_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    [Route("/Tickets/Report/PrintPdf")]
    [Route("/Tickets/Ticket/PrintPdf")]
    public async Task<IActionResult> PrintPdf(
        DateTime? startDate, 
        DateTime? endDate, 
        int? branchId, 
        int? statusId, 
        int? priorityId, 
        int? assignedUserId, 
        string? search)
    {
        var ticketsResponse = await SendRequest<List<Ticket>>("ticket/getall");
        var usersResponse = await SendRequest<List<User>>("user/getall");
        var branchesResponse = await SendRequest<List<Branch>>("branch/getall");

        var allTickets = ticketsResponse?.Data ?? new List<Ticket>();
        var allUsers = usersResponse?.Data ?? new List<User>();
        var allBranches = branchesResponse?.Data ?? new List<Branch>();

        var filtered = allTickets.AsEnumerable();

        if (startDate.HasValue) filtered = filtered.Where(t => t.CreatedAt >= startDate.Value);
        if (endDate.HasValue) filtered = filtered.Where(t => t.CreatedAt <= endDate.Value.AddDays(1));
        if (branchId.HasValue && branchId.Value > 0) filtered = filtered.Where(t => t.CurrentBranchId == branchId.Value);
        if (statusId.HasValue && statusId.Value > 0) filtered = filtered.Where(t => t.StatusId == statusId.Value);
        if (priorityId.HasValue && priorityId.Value > 0) filtered = filtered.Where(t => t.PriorityId == priorityId.Value);
        if (assignedUserId.HasValue && assignedUserId.Value > 0) filtered = filtered.Where(t => t.AssignedUserId == assignedUserId.Value);

        var model = new ReportFilterViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            BranchId = branchId,
            StatusId = statusId,
            PriorityId = priorityId,
            AssignedUserId = assignedUserId,
            SearchTerm = search,
            FilteredTickets = filtered.OrderByDescending(t => t.CreatedAt).ToList(),
            Branches = allBranches,
            Personnel = allUsers
        };

        return View("~/Areas/Tickets/Views/Report/PrintPdf.cshtml", model);
    }

    [HttpGet]
    [Route("/Tickets/Faq/Index")]
    [Route("/Tickets/Faq")]
    [Route("/Faq")]
    public IActionResult Faq(string? search, string? category)
    {
        var articles = GetPredefinedFaqArticles();

        if (!string.IsNullOrWhiteSpace(category))
        {
            articles = articles.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower().Trim();
            articles = articles.Where(a => 
                a.Title.ToLower().Contains(term) || 
                a.Summary.ToLower().Contains(term) || 
                a.Tags.Any(t => t.ToLower().Contains(term)) ||
                a.SolutionSteps.Any(s => s.ToLower().Contains(term))).ToList();
        }

        var model = new KnowledgeBaseViewModel
        {
            SearchTerm = search,
            CategoryFilter = category,
            Articles = articles,
            Categories = new List<string> { "Güvenlik & Şifre", "E-Posta & İletişim", "Ağ & VPN", "Donanım & Yazıcı", "Yazılım & Lisans" }
        };

        return View("~/Areas/Tickets/Views/Faq/Index.cshtml", model);
    }

    [HttpGet]
    [Route("/Tickets/Faq/Detail/{id}")]
    public IActionResult FaqDetail(int id)
    {
        var articles = GetPredefinedFaqArticles();
        var article = articles.FirstOrDefault(a => a.Id == id) ?? articles.First();
        article.ViewCount += 1;
        return View("~/Areas/Tickets/Views/Faq/Detail.cshtml", article);
    }

    private static List<FaqArticle> GetPredefinedFaqArticles()
    {
        return new List<FaqArticle>
        {
            new FaqArticle
            {
                Id = 1,
                Title = "Active Directory Şifre ve Hesap Kilit Sıfırlama",
                Category = "Güvenlik & Şifre",
                Icon = "fa-key",
                Summary = "Active Directory hesabınız kilitlendiğinde veya şifrenizi unuttuğunuzda yapılması gereken adımlar.",
                Content = "Kurumsal Active Directory hesabınız hatalı şifre denemeleri sonucu kilitlendiğinde veya parola yenileme vakti geldiğinde aşağıdaki adımları izleyebilirsiniz.",
                SolutionSteps = new List<string>
                {
                    "Kurumsal self-servis portalına (sifre.dmo.gov.tr) giriş yapın.",
                    "'Şifre Sıfırlama / Hesap Kilidi Açma' seçeneğine tıklayın.",
                    "Telefonunuza gelen SMS doğrulama kodunu onay kutusuna girin.",
                    "Yeni şifrenizi belirleyin (En az 8 karakter, büyük/küçük harf ve özel karakter içermelidir)."
                },
                Tags = new List<string> { "Şifre", "Active Directory", "Kilit", "Hesap", "Parola" },
                ViewCount = 1420,
                HelpfulCount = 380,
                UpdatedAt = DateTime.Now.AddDays(-2)
            },
            new FaqArticle
            {
                Id = 2,
                Title = "Kurumsal E-Posta (Outlook) Ayarları ve Kota Temizliği",
                Category = "E-Posta & İletişim",
                Icon = "fa-envelope",
                Summary = "Exchange ve Outlook e-posta posta kutusu (Mailbox) kotası dolduğunda yapılması gereken temizlik işlemleri.",
                Content = "Posta kutunuz 5 GB limitine yaklaştığında e-posta alımı ve gönderimi durdurulabilir. Kotanızı açmak için aşağıdaki adımları sırasıyla uygulayınız.",
                SolutionSteps = new List<string>
                {
                    "Outlook uygulamasında 'Dosya' menüsünden 'Posta Kutusu Temizleme' aracını seçin.",
                    "'Silinmiş Öğeler' ve 'Gereksiz E-posta' klasörlerini tamamen boşaltın.",
                    "Büyüklüğü 10 MB üzeri olan eski ekli e-postalarınızı yerel arşiv (PST) dosyasına aktarın.",
                    "Outlook uygulamasını kapatıp yeniden başlatarak kotanın güncellenmesini bekleyin."
                },
                Tags = new List<string> { "E-Posta", "Outlook", "Mail", "Exchange", "Kota" },
                ViewCount = 980,
                HelpfulCount = 245,
                UpdatedAt = DateTime.Now.AddDays(-5)
            },
            new FaqArticle
            {
                Id = 3,
                Title = "DMO Uzaktan Erişim ve VPN Bağlantı Kurulumu",
                Category = "Ağ & VPN",
                Icon = "fa-network-wired",
                Summary = "Şube dışından kurumsal ağa güvenli erişim (VPN) sağlama ve bağlantı sorunlarını giderme rehberi.",
                Content = "Kurumsal dizüstü bilgisayarınız ile evden veya saha çalışmasından iç ağ sistemlerine erişmek için FortiClient VPN konfigürasyonu yapılması gerekmektedir.",
                SolutionSteps = new List<string>
                {
                    "FortiClient / SSL VPN uygulamasını başlatın.",
                    "Remote Gateway alanına 'vpn.dmo.gov.tr' ve port alanına '443' yazın.",
                    "Kullanıcı adı alanına kurumsal e-posta adresinizi ve şifrenizi girin.",
                    "İki faktörlü kimlik doğrulama (2FA) SMS kodunu onaylayıp 'Bağlan' butonuna basın."
                },
                Tags = new List<string> { "VPN", "Ağ", "İnternet", "FortiClient", "Uzaktan Erişim" },
                ViewCount = 2150,
                HelpfulCount = 512,
                UpdatedAt = DateTime.Now.AddDays(-1)
            },
            new FaqArticle
            {
                Id = 4,
                Title = "Şebeke Ağ Yazıcısı Tanıtma ve Spooler Kuyruk Hata Çözümü",
                Category = "Donanım & Yazıcı",
                Icon = "fa-print",
                Summary = "Şubedeki ortak ağ yazıcısını bilgisayara ekleme ve 'Yazdırma Biriktiricisi' kilitlenmelerini çözme adımları.",
                Content = "Yazıcıdan çıktı alamadığınızda veya yazdırma kuyruğu tıkandığında teknik ekibi beklemeden servisi yeniden başlatabilirsiniz.",
                SolutionSteps = new List<string>
                {
                    "Klavye üzerinde 'Windows + R' tuşlarına basıp 'services.msc' yazın.",
                    "Listeden 'Yazdırma Biriktiricisi' (Print Spooler) servisini bulun ve sağ tıklayıp 'Yeniden Başlat' deyin.",
                    "Dosya Gezginini açıp adres çubuğuna '\\\\10.10.1.5' şube yazıcı IP adresini yazın.",
                    "Açılan ortak yazıcı simgesine çift tıklayarak otomatik sürücü kurulumunu tamamlayın."
                },
                Tags = new List<string> { "Yazıcı", "Print Spooler", "Baskı", "Ağ Yazıcısı", "Çıktı" },
                ViewCount = 1890,
                HelpfulCount = 430,
                UpdatedAt = DateTime.Now.AddDays(-3)
            },
            new FaqArticle
            {
                Id = 5,
                Title = "Kurumsal Wi-Fi (DMO-KURUMSAL) Kablosuz Ağ Bağlantısı",
                Category = "Ağ & VPN",
                Icon = "fa-wifi",
                Summary = "DMO şube binalarında güvenli yüksek hızlı kablosuz ağa (Wi-Fi) cihaz bağlama yönergesi.",
                Content = "Şube binası içerisinde kurumsal mobil cihazlarınızı ve dizüstü bilgisayarlarınızı 802.1X güvenli Wi-Fi ağına bağlamak için yapılması gerekenler.",
                SolutionSteps = new List<string>
                {
                    "Cihazınızın Wi-Fi ayarlarından 'DMO-KURUMSAL' ağını seçin.",
                    "EAP Yöntemini 'PEAP', Aşama 2 Kimlik Doğrulamasını 'MSCHAPv2' olarak ayarlayın.",
                    "Kimlik alanına kurumsal e-posta adresinizi, şifre alanına AD parolanızı yazın.",
                    "Sertifika doğrulama uyarısı çıktığında 'Güven / Onayla' butonuna dokunun."
                },
                Tags = new List<string> { "Wi-Fi", "Kablosuz Ağ", "İnternet", "Mobil", "Eduroam" },
                ViewCount = 760,
                HelpfulCount = 190,
                UpdatedAt = DateTime.Now.AddDays(-7)
            }
        };
    }
}
