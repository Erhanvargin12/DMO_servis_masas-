namespace WebAPI.Controllers;

/*[ApiController]
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

    [HttpGet("getall")]
    public IActionResult GetAll()
    {
        var result = _talepService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
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

    [HttpGet("yanitlar/{talepId}")]
    public IActionResult GetYanitlar(int talepId)
    {
        var result = _talepService.GetYanitlar(talepId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


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
}*/