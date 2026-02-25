using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.Talepler.Models;

// ── Talep Ana ViewModel ────────────────────────────────────────────────────

public class TalepViewModel
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
    public int Oncelik { get; set; }
    public string OncelikAdi => Oncelik switch { 1 => "Düşük", 2 => "Normal", 3 => "Yüksek", 4 => "Kritik", _ => "—" };
    public string OncelikCss => Oncelik switch { 1 => "badge-muted", 2 => "badge-info", 3 => "badge-warning", 4 => "badge-error", _ => "" };
    public int Durum { get; set; }
    public string DurumAdi => Durum switch { 1 => "Açık", 2 => "İşleniyor", 3 => "Beklemede", 4 => "Kapalı", 5 => "İptal", _ => "—" };
    public string DurumCss => Durum switch { 1 => "badge-success", 2 => "badge-info", 3 => "badge-warning", 4 => "badge-muted", 5 => "badge-error", _ => "" };
    public int OlusturanKullaniciId { get; set; }
    public string OlusturanAdi { get; set; } = string.Empty;
    public int? AtananKullaniciId { get; set; }
    public string? AtananAdi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
    public DateTime? KapanmaTarihi { get; set; }

    public bool Kapali => Durum == 4 || Durum == 5;
}

// ── Talep Ana Sayfa (Tab tabanlı) ──────────────────────────────────────────

public class TalepAnaSayfaViewModel
{
    public string AktifSayfa { get; set; } = "Taleplerim"; // Taleplerim | Bekleyenler | Tamamlananlar | Gorevlerim
    public List<TalepViewModel> Talepler { get; set; } = new();
    public List<TalepKategoriViewModel> Kategoriler { get; set; } = new();
    public TalepOlusturViewModel OlusturForm { get; set; } = new();
    public string? SearchTerm { get; set; }

    // Pagination
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// ── Talep Güncelle (Modal) ─────────────────────────────────────────────────

public class TalepGuncelleViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [MinLength(5, ErrorMessage = "Başlık en az 5 karakter olmalıdır")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [Display(Name = "Açıklama")]
    public string Aciklama { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçilmelidir")]
    [Display(Name = "Kategori")]
    public int KategoriId { get; set; }

    [Display(Name = "Öncelik")]
    public int Oncelik { get; set; } = 2;
}

// ── Talep Liste (eski, geriye dönük uyumluluk) ────────────────────────────

public class TalepListeViewModel
{
    public List<TalepViewModel> Talepler { get; set; } = new();
    public List<TalepKategoriViewModel> Kategoriler { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int? DurumFilter { get; set; }
    public int? OncelikFilter { get; set; }

    // Pagination
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// ── Talep Detay ─────────────────────────────────────────────────────────────

public class TalepDetayViewModel
{
    public TalepViewModel Talep { get; set; } = new();
    public List<TalepYanitViewModel> Yanitlar { get; set; } = new();
    public List<TalepChatViewModel> Chat { get; set; } = new();
    public List<TalepAtamaViewModel> Atamalar { get; set; } = new();
    public List<TalepSevkViewModel> Sevkler { get; set; } = new();
    public List<TalepEkViewModel> Ekler { get; set; } = new();

    // Modal verileri (Atama + Sevk)
    public List<SelectListItem> KullaniciListesi { get; set; } = new();
    public List<SelectListItem> RolListesi { get; set; } = new();
}

// ── Talep Oluştur ──────────────────────────────────────────────────────────

public class TalepOlusturViewModel
{
    [Required(ErrorMessage = "Başlık zorunludur")]
    [MinLength(5, ErrorMessage = "Başlık en az 5 karakter olmalıdır")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [Display(Name = "Açıklama")]
    public string Aciklama { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçilmelidir")]
    [Display(Name = "Kategori")]
    public int KategoriId { get; set; }

    [Display(Name = "Öncelik")]
    public int Oncelik { get; set; } = 2;

    public List<IFormFile>? Ekler { get; set; }

    // Dropdown verileri
    public List<SelectListItem> KategoriListesi { get; set; } = new();
    public List<SelectListItem> OncelikListesi { get; set; } = new()
    {
        new("Düşük", "1"),
        new("Normal", "2"),
        new("Yüksek", "3"),
        new("Kritik", "4")
    };
}

// ── Yanıtla ────────────────────────────────────────────────────────────────

public class TalepYanitlaViewModel
{
    public int TalepId { get; set; }
    public TalepViewModel? Talep { get; set; }

    [Required(ErrorMessage = "Yanıt içeriği zorunludur")]
    [Display(Name = "Yanıt")]
    public string Icerik { get; set; } = string.Empty;

    public List<IFormFile>? Ekler { get; set; }
}

// ── Sevk ───────────────────────────────────────────────────────────────────

public class TalepSevkViewModel2
{
    public int TalepId { get; set; }
    public TalepViewModel? Talep { get; set; }

    [Display(Name = "Hedef Kullanıcı")]
    public int? HedefKullaniciId { get; set; }

    [Display(Name = "Hedef Rol")]
    public int? HedefRolId { get; set; }

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public List<SelectListItem> KullaniciListesi { get; set; } = new();
    public List<SelectListItem> RolListesi { get; set; } = new();
}

// ── Atama ──────────────────────────────────────────────────────────────────

public class TalepAtamaViewModel2
{
    public int TalepId { get; set; }
    public TalepViewModel? Talep { get; set; }

    [Required(ErrorMessage = "Atanacak kullanıcı seçilmelidir")]
    [Display(Name = "Kullanıcı")]
    public int AtananKullaniciId { get; set; }

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public List<SelectListItem> KullaniciListesi { get; set; } = new();
}

// ── İlişkili Veri ViewModels ────────────────────────────────────────────────

public class TalepYanitViewModel
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; }
    public List<TalepEkViewModel> Ekler { get; set; } = new();
}

public class TalepChatViewModel
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int GonderenKullaniciId { get; set; }
    public string GonderenAdi { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; }
    public bool BenimMi { get; set; }
}

public class TalepAtamaViewModel
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int AtayanKullaniciId { get; set; }
    public string AtayanAdi { get; set; } = string.Empty;
    public int AtananKullaniciId { get; set; }
    public string AtananAdi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public DateTime AtamaTarihi { get; set; }
}

public class TalepSevkViewModel
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int SevkEdenKullaniciId { get; set; }
    public string SevkEdenAdi { get; set; } = string.Empty;
    public int? HedefKullaniciId { get; set; }
    public string? HedefKullaniciAdi { get; set; }
    public int? HedefRolId { get; set; }
    public string? HedefRolAdi { get; set; }
    public string? Aciklama { get; set; }
    public DateTime SevkTarihi { get; set; }
}

public class TalepEkViewModel
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int? YanitId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;
    public long DosyaBoyutu { get; set; }
    public string DosyaTipi { get; set; } = string.Empty;
    public int YukleyenKullaniciId { get; set; }
    public string YukleyenAdi { get; set; } = string.Empty;
    public DateTime YuklemeTarihi { get; set; }

    public string BoyutMetni => DosyaBoyutu switch
    {
        < 1024 => $"{DosyaBoyutu} B",
        < 1024 * 1024 => $"{DosyaBoyutu / 1024.0:F1} KB",
        _ => $"{DosyaBoyutu / (1024.0 * 1024):F1} MB"
    };

    public string DosyaIkonu => DosyaTipi switch
    {
        string t when t.Contains("image") => "image",
        string t when t.Contains("pdf") => "pdf",
        string t when t.Contains("word") || t.Contains("document") => "word",
        string t when t.Contains("excel") || t.Contains("spreadsheet") => "excel",
        string t when t.Contains("zip") || t.Contains("compressed") => "zip",
        _ => "file"
    };
}

public class TalepKategoriViewModel
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; }
}

