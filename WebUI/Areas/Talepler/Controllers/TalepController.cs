using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebUI.Areas.Talepler.Models;
using WebUI.Services.Abstract;

namespace WebUI.Areas.Talepler.Controllers;

[Area("Talepler")]
[Authorize]
public class TalepController : Controller
{
    private const int DefaultPageSize = 15;

    private readonly ITalepApiService _talepService;
    private readonly IUserApiService _userService;
    private readonly IRoleApiService _roleService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TalepController> _logger;

    public TalepController(
        ITalepApiService talepService,
        IUserApiService userService,
        IRoleApiService roleService,
        ICurrentUserService currentUser,
        ILogger<TalepController> logger)
    {
        _talepService = talepService;
        _userService = userService;
        _roleService = roleService;
        _currentUser = currentUser;
        _logger = logger;
    }

    private int CurrentUserId =>
        int.TryParse(_currentUser.UserId, out var id) ? id : 0;

    // ── Index yönlendirme ─────────────────────────────────────────────────

    public IActionResult Index() => RedirectToAction(nameof(Taleplerim));

    // ── Taleplerim ────────────────────────────────────────────────────────

    public async Task<IActionResult> Taleplerim(string? search, int page = 1)
    {
        return await RenderAnaSayfa("Taleplerim", search, page,
            talepler => talepler.Where(t => t.OlusturanKullaniciId == CurrentUserId).ToList());
    }

    // ── Bekleyenler (Birimde bekleyen açık/işleniyor/beklemede talepler) ──

    public async Task<IActionResult> Bekleyenler(string? search, int page = 1)
    {
        return await RenderAnaSayfa("Bekleyenler", search, page,
            talepler => talepler.Where(t => t.Durum == 1 || t.Durum == 2 || t.Durum == 3).ToList());
    }

    // ── Tamamlananlar ─────────────────────────────────────────────────────

    public async Task<IActionResult> Tamamlananlar(string? search, int page = 1)
    {
        return await RenderAnaSayfa("Tamamlananlar", search, page,
            talepler => talepler.Where(t => t.Durum == 4 || t.Durum == 5).ToList());
    }

    // ── Görevli Olduğum Talepler ──────────────────────────────────────────

    public async Task<IActionResult> Gorevlerim(string? search, int page = 1)
    {
        return await RenderAnaSayfa("Gorevlerim", search, page,
            talepler => talepler.Where(t => t.AtananKullaniciId == CurrentUserId).ToList());
    }

    private async Task<IActionResult> RenderAnaSayfa(
        string aktifSayfa,
        string? search,
        int page,
        Func<List<TalepViewModel>, List<TalepViewModel>> filtre)
    {
        var tumTalepler = await _talepService.GetAllAsync();
        var kategoriler = await _talepService.GetKategorilerAsync();

        var filtered = filtre(tumTalepler);

        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(t =>
                t.Baslik.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Aciklama.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.OlusturanAdi.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        var totalCount = filtered.Count;
        var pagedItems = filtered
            .OrderByDescending(t => t.OlusturmaTarihi)
            .Skip((page - 1) * DefaultPageSize)
            .Take(DefaultPageSize)
            .ToList();

        var model = new TalepAnaSayfaViewModel
        {
            AktifSayfa = aktifSayfa,
            Talepler = pagedItems,
            Kategoriler = kategoriler,
            OlusturForm = new TalepOlusturViewModel
            {
                KategoriListesi = kategoriler
                    .Select(k => new SelectListItem(k.Ad, k.Id.ToString()))
                    .ToList()
            },
            SearchTerm = search,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = DefaultPageSize
        };

        return View("Talepler", model);
    }

    // ── Detay ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Detay(int id)
    {
        var talep = await _talepService.GetByIdAsync(id);
        if (talep == null)
        {
            TempData["ErrorMessage"] = "Talep bulunamadı.";
            return RedirectToAction(nameof(Taleplerim));
        }

        var yanitlar = await _talepService.GetYanitlarAsync(id);
        var chat = await _talepService.GetChatAsync(id);
        var atamalar = await _talepService.GetAtamalarAsync(id);
        var sevkler = await _talepService.GetSevklerAsync(id);
        var ekler = await _talepService.GetEklerAsync(id);
        var kullanicilar = await _userService.GetAllAsync();
        var roller = await _roleService.GetAllAsync();

        foreach (var yanit in yanitlar)
            yanit.Ekler = ekler.Where(e => e.YanitId == yanit.Id).ToList();

        var meId = CurrentUserId;
        foreach (var msg in chat)
            msg.BenimMi = msg.GonderenKullaniciId == meId;

        var model = new TalepDetayViewModel
        {
            Talep = talep,
            Yanitlar = yanitlar,
            Chat = chat,
            Atamalar = atamalar,
            Sevkler = sevkler,
            Ekler = ekler.Where(e => e.YanitId == null).ToList(),
            KullaniciListesi = kullanicilar
                .Where(u => u.Status && u.Id != meId)
                .Select(u => new SelectListItem($"{u.FullName} ({u.Mail})", u.Id.ToString()))
                .ToList(),
            RolListesi = roller
                .Select(r => new SelectListItem(r.Name, r.Id.ToString()))
                .ToList()
        };

        return View(model);
    }

    // ── Oluştur (Modal POST) ───────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(TalepOlusturViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Taleplerim));
        }

        var (success, message) = await _talepService.AddAsync(model, CurrentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Taleplerim));
        }

        if (model.Ekler != null && model.Ekler.Count > 0)
        {
            var talepler = await _talepService.GetAllAsync(olusturanId: CurrentUserId);
            var yeniTalep = talepler.OrderByDescending(t => t.OlusturmaTarihi).FirstOrDefault();
            if (yeniTalep != null)
                foreach (var dosya in model.Ekler)
                    await _talepService.EkEkleAsync(yeniTalep.Id, null, CurrentUserId, dosya);
        }

        TempData["SuccessMessage"] = message ?? "Talep başarıyla oluşturuldu!";
        return RedirectToAction(nameof(Taleplerim));
    }

    // ── Güncelle (Modal POST) ──────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guncelle(TalepGuncelleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Taleplerim));
        }

        var mevcut = await _talepService.GetByIdAsync(model.Id);
        if (mevcut == null)
        {
            TempData["ErrorMessage"] = "Talep bulunamadı.";
            return RedirectToAction(nameof(Taleplerim));
        }

        mevcut.Baslik = model.Baslik;
        mevcut.Aciklama = model.Aciklama;
        mevcut.KategoriId = model.KategoriId;
        mevcut.Oncelik = model.Oncelik;

        var (success, message) = await _talepService.UpdateAsync(mevcut);
        if (success)
            TempData["SuccessMessage"] = message ?? "Talep güncellendi!";
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Taleplerim));
    }

    // ── Sil (Modal POST) ──────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int talepId)
    {
        var (success, message) = await _talepService.DeleteAsync(talepId);
        if (success)
            TempData["SuccessMessage"] = message ?? "Talep silindi.";
        else
            TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Taleplerim));
    }

    // ── Yanıtla (Modal POST - Detay sayfasından) ──────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Yanitla(int talepId, string icerik, List<IFormFile>? ekler)
    {
        if (string.IsNullOrWhiteSpace(icerik))
        {
            TempData["ErrorMessage"] = "Yanıt içeriği boş olamaz.";
            return RedirectToAction(nameof(Detay), new { id = talepId });
        }

        var (success, message) = await _talepService.YanitEkleAsync(talepId, CurrentUserId, icerik);

        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Detay), new { id = talepId });
        }

        if (ekler != null && ekler.Count > 0)
        {
            var yanitlar = await _talepService.GetYanitlarAsync(talepId);
            var sonYanit = yanitlar.OrderByDescending(y => y.OlusturmaTarihi).FirstOrDefault();
            if (sonYanit != null)
                foreach (var dosya in ekler)
                    await _talepService.EkEkleAsync(talepId, sonYanit.Id, CurrentUserId, dosya);
        }

        TempData["SuccessMessage"] = message ?? "Yanıt eklendi!";
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    // ── Chat Mesajı Ekle ───────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChatEkle(int talepId, string mesaj)
    {
        if (string.IsNullOrWhiteSpace(mesaj))
        {
            TempData["ErrorMessage"] = "Mesaj boş olamaz.";
            return RedirectToAction(nameof(Detay), new { id = talepId });
        }

        var (success, message) = await _talepService.ChatEkleAsync(talepId, CurrentUserId, mesaj);
        if (!success) TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    // ── Sevk (Modal POST - Detay sayfasından) ─────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sevk(int talepId, int? hedefKullaniciId, int? hedefRolId, string? aciklama)
    {
        if (!hedefKullaniciId.HasValue && !hedefRolId.HasValue)
        {
            TempData["ErrorMessage"] = "Hedef kullanıcı veya rol seçilmelidir.";
            return RedirectToAction(nameof(Detay), new { id = talepId });
        }

        var (success, message) = await _talepService.SevketAsync(
            talepId, CurrentUserId, hedefKullaniciId, hedefRolId, aciklama);

        if (success)
            TempData["SuccessMessage"] = message ?? "Talep sevk edildi!";
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    // ── Atama (Modal POST - Detay sayfasından) ────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Atama(int talepId, int atananKullaniciId, string? aciklama)
    {
        var (success, message) = await _talepService.KullaniciAtaAsync(
            talepId, CurrentUserId, atananKullaniciId, aciklama);

        if (success)
            TempData["SuccessMessage"] = message ?? "Kullanıcı atandı!";
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    // ── Ek İndir / Sil / Ekle ─────────────────────────────────────────────

    public async Task<IActionResult> EkIndir(int ekId)
    {
        var (success, data, dosyaAdi, contentType) = await _talepService.EkIndirAsync(ekId);
        if (!success || data == null)
        {
            TempData["ErrorMessage"] = "Dosya indirilemedi.";
            return RedirectToAction(nameof(Taleplerim));
        }
        return File(data, contentType ?? "application/octet-stream", dosyaAdi ?? "dosya");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EkSil(int ekId, int talepId)
    {
        var (success, message) = await _talepService.EkSilAsync(ekId, CurrentUserId);
        if (success) TempData["SuccessMessage"] = message ?? "Dosya silindi.";
        else TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EkEkle(int talepId, IFormFile? dosya)
    {
        if (dosya == null || dosya.Length == 0)
        {
            TempData["ErrorMessage"] = "Dosya seçilmedi.";
            return RedirectToAction(nameof(Detay), new { id = talepId });
        }

        var (success, message) = await _talepService.EkEkleAsync(talepId, null, CurrentUserId, dosya);
        if (success) TempData["SuccessMessage"] = message ?? "Dosya yüklendi.";
        else TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    // ── Durum İşlemleri ────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kapat(int talepId)
    {
        var (success, message) = await _talepService.KapatAsync(talepId, CurrentUserId);
        if (success) TempData["SuccessMessage"] = message ?? "Talep kapatıldı.";
        else TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iptal(int talepId)
    {
        var (success, message) = await _talepService.IptalAsync(talepId, CurrentUserId);
        if (success) TempData["SuccessMessage"] = message ?? "Talep iptal edildi.";
        else TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int talepId, int durum)
    {
        var (success, message) = await _talepService.DurumGuncelleAsync(talepId, durum, CurrentUserId);
        if (success) TempData["SuccessMessage"] = message ?? "Durum güncellendi.";
        else TempData["ErrorMessage"] = message;
        return RedirectToAction(nameof(Detay), new { id = talepId });
    }
}
