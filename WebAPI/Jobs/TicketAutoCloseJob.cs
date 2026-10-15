using DataAccess.Abstract.TicketDal;

namespace WebAPI.Jobs;

/// <summary>
/// "Çözüldü" (StatusId=5) statusüne geçen ve belirlenen süre içinde
/// talebi açan kullanıcıdan yanıt gelmeyen ticketları otomatik kapatır (StatusId=6).
/// </summary>
public class TicketAutoCloseJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketAutoCloseJob> _logger;
    private readonly int _autoCloseDays;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public TicketAutoCloseJob(
        IServiceScopeFactory scopeFactory,
        ILogger<TicketAutoCloseJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _autoCloseDays = configuration.GetValue<int>("Ticket:AutoCloseDays", 3);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketAutoCloseJob başlatıldı. Bekleme süresi: {Days} gün.", _autoCloseDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            await OtomatikKapat(stoppingToken);
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task OtomatikKapat(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var ticketDal = scope.ServiceProvider.GetRequiredService<ITicketDal>();
            var responseDal = scope.ServiceProvider.GetRequiredService<ITicketResponseDal>();

            var sinirTarihi = DateTime.Now.AddDays(-_autoCloseDays);

            // StatusId=5 (Çözüldü), ResolvedAt belirtilen günü aşmış talepler
            var adaylar = await ticketDal.GetAllAsync(t =>
                t.StatusId == 5 &&
                t.ResolvedAt.HasValue &&
                t.ResolvedAt.Value <= sinirTarihi);

            foreach (var ticket in adaylar)
            {
                // Son cevabın bu süreçte gelmediğini doğrula
                var sonCevaplar = await responseDal.GetAllAsync(r =>
                    r.TicketId == ticket.Id &&
                    r.CreatedAt > ticket.ResolvedAt);

                if (!sonCevaplar.Any())
                {
                    ticket.StatusId = 6;        // Kapatıldı
                    ticket.ClosedAt = DateTime.Now;
                    await ticketDal.UpdateAsync(ticket);
                    _logger.LogInformation("Ticket #{TicketNo} otomatik kapatıldı.", ticket.TicketNo);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TicketAutoCloseJob çalıştırılırken hata oluştu.");
        }
    }
}
