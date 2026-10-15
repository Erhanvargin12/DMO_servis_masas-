using Core.Communication;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;

namespace WebUI.Areas.Tickets.Controllers
{
    [Area("Tickets")]
    [Authorize]
    public class SupplierController : BaseController
    {
        private readonly IServiceCommunication _service;

        public SupplierController(IApiClient apiClient, IServiceCommunication service) : base(apiClient)
        {
            _service = service;
        }

        [HttpGet]
        [Route("/Tickets/Supplier/Index")]
        [Route("/Tickets/Supplier")]
        [Route("/Tedarikci/Portal")]
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Company" && !User.IsInRole("SupplierCompany"))
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz bulunmamaktadır. Tedarikçi Portalı sadece kayıtlı yüklenici firmalar içindir.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var allTicketsResponse = await _service.GetAsync<DataResult<List<Ticket>>>("KIP", "ticket/getall");
            var allTickets = allTicketsResponse?.Data ?? new List<Ticket>();

            // Tedarikçi Firmaya sevk edilmiş / atanmış olan dış servis taleplerini süz
            var supplierTickets = allTickets
                .Where(t => t.Title.Contains("Yazıcı", StringComparison.OrdinalIgnoreCase) ||
                            t.Title.Contains("Fiber", StringComparison.OrdinalIgnoreCase) ||
                            t.Title.Contains("Donanım", StringComparison.OrdinalIgnoreCase) ||
                            t.Title.Contains("Sunucu", StringComparison.OrdinalIgnoreCase) ||
                            t.Description.Contains("servis", StringComparison.OrdinalIgnoreCase) ||
                            t.Description.Contains("parça", StringComparison.OrdinalIgnoreCase) ||
                            t.PriorityId >= 3)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                supplierTickets = supplierTickets.Where(t => 
                    t.Title.ToLowerInvariant().Contains(s) || 
                    t.TicketNo.ToLowerInvariant().Contains(s) || 
                    t.Description.ToLowerInvariant().Contains(s)).ToList();
            }

            var model = new TicketViewModel
            {
                FilteredTickets = supplierTickets,
                OwnCount = supplierTickets.Count,
                SearchTerm = search,
                StatusFilter = status
            };

            ViewBag.CompanyName = HttpContext.Session.GetString("CompanyName") ?? "Xerox & HP Kurumsal Teknik Servis A.Ş.";
            ViewBag.CompanyEmail = HttpContext.Session.GetString("CompanyEmail") ?? "destek@xerox.com.tr";

            return View(model);
        }

        [HttpPost]
        [Route("/Tickets/Supplier/UpdateStatus")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateStatus([FromForm] int ticketId, [FromForm] int statusId, [FromForm] string? serviceNote)
        {
            if (ticketId > 0)
            {
                var ticketResponse = await _service.GetAsync<DataResult<TicketDto>>("KIP", $"ticket/getticketdtobyid/{ticketId}");
                if (ticketResponse?.Data?.Ticket != null)
                {
                    ticketResponse.Data.Ticket.StatusId = statusId;
                    ticketResponse.Data.Ticket.UpdatedAt = DateTime.Now;
                    if (statusId == 5 || statusId == 6)
                    {
                        ticketResponse.Data.Ticket.ResolvedAt = DateTime.Now;
                        ticketResponse.Data.Ticket.ClosedAt = DateTime.Now;
                    }
                    await _service.PostAsync<TicketDto, DataResult<TicketDto>>("KIP", "ticket/update", ticketResponse.Data);
                }

                TempData["SuccessMessage"] = "Saha / Tedarikçi servis durumu başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Tickets/Supplier/CreateTicket")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateTicket([FromForm] string title, [FromForm] string description, [FromForm] int priorityId, [FromForm] string? categoryName)
        {
            var companyName = HttpContext.Session.GetString("CompanyName") ?? "Tedarikçi Yüklenici Firma";
            var companyEmail = HttpContext.Session.GetString("CompanyEmail") ?? "destek@firma.com";

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Lütfen talep başlığı ve detaylı açıklamasını doldurunuz.";
                return RedirectToAction("Index");
            }

            // 1. Çağrı Merkezi Şube ID'sini dinamik olarak sorgula
            int callCenterBranchId = 1022;
            try
            {
                var branchesResponse = await _service.GetAsync<DataResult<List<Branch>>>("KIP", "branch/getall");
                if (branchesResponse?.Data != null && branchesResponse.Data.Any())
                {
                    var ccBranch = branchesResponse.Data.FirstOrDefault(b => 
                        (b.Name != null && b.Name.Contains("Çağrı Merkezi", StringComparison.OrdinalIgnoreCase)) ||
                        (b.DepartmentId != null && (b.DepartmentId == "3999" || b.DepartmentId == "CAGRI"))
                    );
                    if (ccBranch != null)
                    {
                        callCenterBranchId = ccBranch.Id;
                    }
                }
            }
            catch
            {
                // Fallback
            }

            // 2. Yeni Ticket DTO nesnesi oluştur ve Çağrı Merkezine yönlendir
            var newTicket = new Ticket
            {
                TicketNo = "TKT-SUP-" + DateTime.Now.ToString("yyyyMMdd-HHmm"),
                Title = $"[TEDARİKÇİ TALEP] {title.Trim()}",
                Description = $"Firma: {companyName}\nE-Posta: {companyEmail}\nKategori: {categoryName ?? "Dış Hizmet / Saha"}\n\nDetay:\n{description.Trim()}",
                CreatedById = 1,
                CurrentBranchId = callCenterBranchId,
                PriorityId = priorityId > 0 ? priorityId : 2,
                StatusId = 1,
                CreatedAt = DateTime.Now
            };

            var dto = new TicketDto
            {
                Ticket = newTicket
            };

            await _service.PostAsync<TicketDto, DataResult<TicketDto>>("KIP", "ticket/add", dto);
            TempData["SuccessMessage"] = "Talebiniz Çağrı Merkezi Müdürlüğü'ne başarıyla iletilmiştir. Temsilcilerimiz talebinizi inceleyecektir.";

            return RedirectToAction("Index");
        }
    }
}
