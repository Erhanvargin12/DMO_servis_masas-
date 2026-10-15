using Business.Abstract;
using Business.Abstract.TicketServices;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using System.Linq.Expressions;

namespace Business.Concrete.TicketManagers
{
    public class TicketManager : ITicketService
    {
        private readonly ITicketDal _ticketDal;
        private readonly ITicketAttachmentDal _attachmentDal;
        private readonly ITicketRoutingService _routingService;
        private readonly ITicketAiService _ticketAiService;

        public TicketManager(
            ITicketDal ticketDal,
            ITicketAttachmentDal attachmentDal,
            ITicketRoutingService routingService,
            ITicketAiService ticketAiService)
        {
            _ticketDal = ticketDal;
            _attachmentDal = attachmentDal;
            _routingService = routingService;
            _ticketAiService = ticketAiService;
        }

        public async Task<IDataResult<TicketDto>> AddAsync(TicketDto dto)
        {
            if (dto.Ticket != null)
            {
                // Yapay zeka ile talep içeriğini analiz et
                var aiResult = await _ticketAiService.AnalyzeTicketAsync(dto.Ticket.Title, dto.Ticket.Description);
                if (aiResult.Success && aiResult.Data != null)
                {
                    // Yapay Zeka analiz sonuçlarını bilet nesnesine kaydet
                    dto.Ticket.AiSummary = aiResult.Data.Summary;
                    dto.Ticket.AiSuggestedAction = aiResult.Data.SuggestedAction;
                    dto.Ticket.AiSentiment = aiResult.Data.Sentiment;

                    // Aciliyet haritalaması (Örnek: 1: Düşük, 2: Orta, 3: Yüksek, 4: Kritik)
                    if (dto.Ticket.PriorityId == 0)
                    {
                        dto.Ticket.PriorityId = aiResult.Data.Priority switch
                        {
                            "Düşük" => 1,
                            "Orta" => 2,
                            "Yüksek" => 3,
                            "Kritik" => 4,
                            _ => 2
                        };
                    }
                }
            }

            await _ticketDal.AddAsync(dto.Ticket);

            await _routingService.RouteOnCreateAsync(dto.Ticket, dto.Ticket.CreatedById);

            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var attachment in dto.Attachments)
                {
                    attachment.TicketId = dto.Ticket.Id;
                    await _attachmentDal.AddAsync(attachment);
                }
            }

            return new SuccessDataResult<TicketDto>(dto, Messages.TicketCreated);
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            IResult result = await BusinessRules.RunAsync(() => CheckIfTicketExistsAsync(id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = await _ticketDal.GetAsync(u => u.Id == id);
            await _ticketDal.DeleteAsync(existingTicket);
            return new SuccessResult(Messages.TicketDeleted);
        }

        public async Task<IDataResult<List<Ticket>>> GetAllAsync(Expression<Func<Ticket, bool>>? filter = null)
        {
            var list = await _ticketDal.GetAllAsync(filter);
            if (filter == null && list.Count < 3)
            {
                await SeedDefaultTickets();
                list = await _ticketDal.GetAllAsync(filter);
            }
            return new SuccessDataResult<List<Ticket>>(list, Messages.TicketsListed);
        }

        private async Task SeedDefaultTickets()
        {
            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    TicketNo = $"TKT-2026-00101",
                    Title = "EBYS Yetki ve Modül Entegrasyonu Arızası",
                    Description = "Elektronik Belge Yönetim Sistemi üzerinde e-imza modülü 500 sunucu hatası veriyor ve evrak onaylama işlemi durdu.",
                    CreatedById = 1,
                    CurrentBranchId = 102, // Yazılım Şube
                    StatusId = 1, // Açık
                    PriorityId = 3, // Yüksek
                    CreatedAt = DateTime.Now.AddHours(-3)
                },
                new Ticket
                {
                    TicketNo = $"TKT-2026-00102",
                    Title = "Ana Lokasyon Fiber Ağ Kesintisi ve Switch Erişimsizliği",
                    Description = "B Bina 3. Kat kenar anahtar (Switch) bağlantısı koptu. İlgili kattaki 45 personel ağa ve internete erişemiyor.",
                    CreatedById = 1,
                    CurrentBranchId = 105, // Sistem ve Ağ Şube
                    StatusId = 3, // İşlemde
                    PriorityId = 4, // Kritik
                    CreatedAt = DateTime.Now.AddHours(-5)
                },
                new Ticket
                {
                    TicketNo = $"TKT-2026-00103",
                    Title = "Yazıcı Toner ve Çok İşlevli Tarayıcı Kurulumu",
                    Description = "Donanım Şube Müdürlüğü birimine teslim edilen çok işlevli ağ tarayıcısının IP tanımlaması ve sürücü kurulumu talebi.",
                    CreatedById = 1,
                    CurrentBranchId = 106, // Donanım Şube
                    AssignedUserId = 2,
                    StatusId = 2, // Atandı
                    PriorityId = 2, // Orta
                    CreatedAt = DateTime.Now.AddHours(-8)
                },
                new Ticket
                {
                    TicketNo = $"TKT-2026-00104",
                    Title = "Aylık Veri Analizi ve Rapor Excel Dışa Aktarım Hatası",
                    Description = "İstatistik Şube Müdürlüğü portalında aylık veri raporu indirilirken timeout alınıyor. Veritabanı sorgu optimizasyonu gerekiyor.",
                    CreatedById = 1,
                    CurrentBranchId = 103, // İstatistik Şube
                    StatusId = 4, // Beklemede
                    PriorityId = 2, // Orta
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new Ticket
                {
                    TicketNo = $"TKT-2026-00105",
                    Title = "Yapay Zeka Destekli Talep Akıllı Otomasyon Entegrasyon Talebi",
                    Description = "Gelen kurumsal taleplerin Gemini AI yapay zeka servisiyle otomatik kategorize edilmesi ve ilgili birimlere akıllı sevk edilmesi.",
                    CreatedById = 1,
                    CurrentBranchId = 104, // Fonksiyonel Şube
                    StatusId = 3, // İşlemde
                    PriorityId = 3, // Yüksek
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Ticket
                {
                    TicketNo = $"TKT-2026-00106",
                    Title = "VIP Konferans Salonu Görüntü Aktarımı ve Ses Sistemi Kurulumu",
                    Description = "Başkanlık VIP Toplantı Salonundaki hibrit toplantı donanımları ve projeksiyon kablosuz aktarım testi başarıyla tamamlandı.",
                    CreatedById = 1,
                    CurrentBranchId = 101, // Başkanlık
                    StatusId = 5, // Çözüldü
                    PriorityId = 4, // Kritik
                    CreatedAt = DateTime.Now.AddDays(-3),
                    ResolvedAt = DateTime.Now.AddDays(-1),
                    ClosedAt = DateTime.Now.AddDays(-1)
                }
            };

            foreach (var t in tickets)
            {
                var existing = await _ticketDal.GetAsync(x => x.TicketNo == t.TicketNo);
                if (existing == null)
                {
                    await _ticketDal.AddAsync(t);
                }
            }
        }

        public async Task<IDataResult<Ticket>> GetByIdAsync(int id)
        {
            return new SuccessDataResult<Ticket>(await _ticketDal.GetAsync(t => t.Id == id), Messages.TicketsListed);
        }

        public async Task<IDataResult<TicketDto>> GetTicketDtoByIdAsync(int id)
        {
            var ticketDto = await _ticketDal.GetTicketDtoByIdAsync(id);
            if (ticketDto?.Ticket != null && string.IsNullOrWhiteSpace(ticketDto.Ticket.AiSummary))
            {
                try
                {
                    var aiResult = await _ticketAiService.AnalyzeTicketAsync(ticketDto.Ticket.Title, ticketDto.Ticket.Description);
                    if (aiResult.Success && aiResult.Data != null)
                    {
                        ticketDto.Ticket.AiSummary = aiResult.Data.Summary;
                        ticketDto.Ticket.AiSuggestedAction = aiResult.Data.SuggestedAction;
                        ticketDto.Ticket.AiSentiment = aiResult.Data.Sentiment;
                    }
                }
                catch
                {
                    // AI dinamik analiz hatasında devam et
                }
            }
            return new SuccessDataResult<TicketDto>(ticketDto, Messages.TicketsListed);
        }

        /// <summary>Birimin aktif bekleyen talepleri (sıradaki sorumlu şube)</summary>
        public async Task<IDataResult<List<Ticket>>> GetByBranchAsync(int branchId)
        {
            int[] aktifStatuler = { 1, 2, 3, 4 };

            var liste = await _ticketDal.GetAllAsync(t =>
                t.CurrentBranchId == branchId &&
                t.StatusId.HasValue &&
                aktifStatuler.Contains(t.StatusId.Value));

            return new SuccessDataResult<List<Ticket>>(liste, Messages.TicketsListed);
        }

        /// <summary>Kullanıcının şu anki veya geçmişteki atamalarındaki tüm talepler</summary>
        public async Task<IDataResult<List<Ticket>>> GetAssignedAsync(int userId)
        {
            var liste = await _ticketDal.GetAllAsync(t => t.AssignedUserId == userId);
            return new SuccessDataResult<List<Ticket>>(liste, Messages.TicketsListed);
        }

        public async Task<IResult> UpdateAsync(Ticket ticket)
        {
            IResult result = await BusinessRules.RunAsync(() => CheckIfTicketExistsAsync(ticket.Id));

            if (result != null)
            {
                return result;
            }

            var existingTicket = await _ticketDal.GetAsync(u => u.Id == ticket.Id);
            if (existingTicket != null)
            {
                if (!string.IsNullOrWhiteSpace(ticket.Title)) existingTicket.Title = ticket.Title;
                if (!string.IsNullOrWhiteSpace(ticket.Description)) existingTicket.Description = ticket.Description;
                if (ticket.AssignedUserId.HasValue && ticket.AssignedUserId.Value > 0) existingTicket.AssignedUserId = ticket.AssignedUserId;
                if (ticket.CurrentBranchId > 0) existingTicket.CurrentBranchId = ticket.CurrentBranchId;
                if (ticket.StatusId.HasValue && ticket.StatusId.Value > 0) existingTicket.StatusId = ticket.StatusId;
                if (ticket.PriorityId > 0) existingTicket.PriorityId = ticket.PriorityId;
                existingTicket.UpdatedAt = DateTime.Now;
                if (ticket.StatusId == 5 || ticket.StatusId == 6)
                {
                    existingTicket.ClosedAt = DateTime.Now;
                }

                await _ticketDal.UpdateAsync(existingTicket);
            }
            return new SuccessResult(Messages.TicketUpdated);
        }

        private async Task<IResult> CheckIfTicketExistsAsync(int id)
        {
            var result = await _ticketDal.GetAsync(u => u.Id == id);
            if (result == null)
            {
                return new ErrorResult(Messages.TicketNotFound);
            }
            return new SuccessResult();
        }
    }
}
