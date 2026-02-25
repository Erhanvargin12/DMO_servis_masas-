using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TalepController : ControllerBase
{
    private readonly ITalepService _talepService;
    private readonly IWebHostEnvironment _env;

    public TalepController(ITalepService talepService, IWebHostEnvironment env)
    {
        _talepService = talepService;
        _env = env;
    }

    // ── Talep CRUD ─────────────────────────────────────────────────────────

    [HttpGet("getall")]
    public IActionResult GetAll([FromQuery] int? olusturanId, [FromQuery] int? atananId, [FromQuery] int? durum)
    {
        var result = _talepService.GetAll(
            olusturanId,
            atananId,
            durum.HasValue ? (TalepDurum)durum.Value : null);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _talepService.GetById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("add")]
    public IActionResult Add([FromBody] Talep talep)
    {
        var result = _talepService.TalepOlustur(talep);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update([FromBody] Talep talep)
    {
        var result = _talepService.TalepGuncelle(talep);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete([FromBody] IdDto dto)
    {
        var result = _talepService.TalepSil(dto.Id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("kapat")]
    public IActionResult Kapat([FromBody] TalepIslemDto dto)
    {
        var result = _talepService.TalepKapat(dto.TalepId, dto.KullaniciId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("iptal")]
    public IActionResult Iptal([FromBody] TalepIslemDto dto)
    {
        var result = _talepService.TalepIptal(dto.TalepId, dto.KullaniciId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("durum")]
    public IActionResult DurumGuncelle([FromBody] TalepDurumDto dto)
    {
        var result = _talepService.DurumGuncelle(dto.TalepId, (TalepDurum)dto.Durum, dto.KullaniciId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Yanıt ──────────────────────────────────────────────────────────────

    [HttpGet("yanitlar/{talepId}")]
    public IActionResult GetYanitlar(int talepId)
    {
        var result = _talepService.GetYanitlar(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("yanitekle")]
    public IActionResult YanitEkle([FromBody] TalepYanit yanit)
    {
        var result = _talepService.YanitEkle(yanit);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("yanitsil")]
    public IActionResult YanitSil([FromBody] YanitIslemDto dto)
    {
        var result = _talepService.YanitSil(dto.YanitId, dto.KullaniciId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Chat ───────────────────────────────────────────────────────────────

    [HttpGet("chat/{talepId}")]
    public IActionResult GetChat(int talepId)
    {
        var result = _talepService.GetChatMesajlari(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("chatekle")]
    public IActionResult ChatEkle([FromBody] TalepChat chat)
    {
        var result = _talepService.ChatMesajEkle(chat);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Atama ──────────────────────────────────────────────────────────────

    [HttpGet("atamalar/{talepId}")]
    public IActionResult GetAtamalar(int talepId)
    {
        var result = _talepService.GetAtamalar(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ata")]
    public IActionResult KullaniciAta([FromBody] TalepAtama atama)
    {
        var result = _talepService.KullaniciAta(atama);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Sevk ───────────────────────────────────────────────────────────────

    [HttpGet("sevkler/{talepId}")]
    public IActionResult GetSevkler(int talepId)
    {
        var result = _talepService.GetSevkler(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("sevket")]
    public IActionResult Sevket([FromBody] TalepSevk sevk)
    {
        var result = _talepService.TalepSevket(sevk);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Ek (Dosya) ─────────────────────────────────────────────────────────

    [HttpGet("ekler/{talepId}")]
    public IActionResult GetEkler(int talepId)
    {
        var result = _talepService.GetEkler(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ekekle")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> EkEkle([FromForm] EkEkleDto dto)
    {
        if (dto.Dosya == null || dto.Dosya.Length == 0)
            return BadRequest(new { Success = false, Message = "Dosya seçilmedi." });

        // Uzantı kontrolü
        var izinliUzantilar = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt", ".zip" };
        var uzanti = Path.GetExtension(dto.Dosya.FileName).ToLower();
        if (!izinliUzantilar.Contains(uzanti))
            return BadRequest(new { Success = false, Message = "İzin verilmeyen dosya türü." });

        // Dosyayı kaydet
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "talepler", dto.TalepId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var benzersizAd = $"{Guid.NewGuid()}{uzanti}";
        var dosyaYolu = Path.Combine(uploadsDir, benzersizAd);

        await using var stream = new FileStream(dosyaYolu, FileMode.Create);
        await dto.Dosya.CopyToAsync(stream);

        var relativePath = $"/uploads/talepler/{dto.TalepId}/{benzersizAd}";

        var ek = new TalepEk
        {
            TalepId = dto.TalepId,
            YanitId = dto.YanitId,
            DosyaAdi = dto.Dosya.FileName,
            DosyaYolu = relativePath,
            DosyaBoyutu = dto.Dosya.Length,
            DosyaTipi = dto.Dosya.ContentType,
            YukleyenKullaniciId = dto.KullaniciId
        };

        var result = _talepService.EkEkle(ek);
        if (!result.Success)
        {
            System.IO.File.Delete(dosyaYolu);
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("eksil")]
    public IActionResult EkSil([FromBody] EkSilDto dto)
    {
        var ekResult = _talepService.GetEkById(dto.EkId);
        if (!ekResult.Success)
            return NotFound(ekResult);

        // Fiziksel dosyayı sil
        var dosyaYolu = Path.Combine(_env.WebRootPath, ekResult.Data.DosyaYolu.TrimStart('/'));
        if (System.IO.File.Exists(dosyaYolu))
            System.IO.File.Delete(dosyaYolu);

        var result = _talepService.EkSil(dto.EkId, dto.KullaniciId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("ekindir/{ekId}")]
    public IActionResult EkIndir(int ekId)
    {
        var result = _talepService.GetEkById(ekId);
        if (!result.Success) return NotFound(result);

        var ek = result.Data;
        var dosyaYolu = Path.Combine(_env.WebRootPath, ek.DosyaYolu.TrimStart('/'));
        if (!System.IO.File.Exists(dosyaYolu))
            return NotFound(new { Success = false, Message = "Dosya bulunamadı." });

        var bytes = System.IO.File.ReadAllBytes(dosyaYolu);
        return File(bytes, ek.DosyaTipi, ek.DosyaAdi);
    }

    // ── Kategori ───────────────────────────────────────────────────────────

    [HttpGet("kategoriler")]
    public IActionResult GetKategoriler()
    {
        var result = _talepService.GetKategoriler();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("kategoriekle")]
    public IActionResult KategoriEkle([FromBody] TalepKategori kategori)
    {
        var result = _talepService.KategoriEkle(kategori);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("kategoriguncelle")]
    public IActionResult KategoriGuncelle([FromBody] TalepKategori kategori)
    {
        var result = _talepService.KategoriGuncelle(kategori);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("kategorisil")]
    public IActionResult KategoriSil([FromBody] IdDto dto)
    {
        var result = _talepService.KategoriSil(dto.Id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────────

public record IdDto(int Id);
public record TalepIslemDto(int TalepId, int KullaniciId);
public record TalepDurumDto(int TalepId, int Durum, int KullaniciId);
public record YanitIslemDto(int YanitId, int KullaniciId);
public record EkSilDto(int EkId, int KullaniciId);

public class EkEkleDto
{
    public int TalepId { get; set; }
    public int? YanitId { get; set; }
    public int KullaniciId { get; set; }
    public IFormFile? Dosya { get; set; }
}

