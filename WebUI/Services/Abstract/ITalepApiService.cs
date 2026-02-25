using WebUI.Areas.Talepler.Models;

namespace WebUI.Services.Abstract;

public interface ITalepApiService
{
    // Talep
    Task<List<TalepViewModel>> GetAllAsync(int? olusturanId = null, int? atananId = null, int? durum = null);
    Task<TalepViewModel?> GetByIdAsync(int id);
    Task<(bool Success, string? Message)> AddAsync(TalepOlusturViewModel model, int kullaniciId);
    Task<(bool Success, string? Message)> UpdateAsync(TalepViewModel model);
    Task<(bool Success, string? Message)> DeleteAsync(int id);
    Task<(bool Success, string? Message)> KapatAsync(int talepId, int kullaniciId);
    Task<(bool Success, string? Message)> IptalAsync(int talepId, int kullaniciId);
    Task<(bool Success, string? Message)> DurumGuncelleAsync(int talepId, int durum, int kullaniciId);

    // Yanıt
    Task<List<TalepYanitViewModel>> GetYanitlarAsync(int talepId);
    Task<(bool Success, string? Message)> YanitEkleAsync(int talepId, int kullaniciId, string icerik);

    // Chat
    Task<List<TalepChatViewModel>> GetChatAsync(int talepId);
    Task<(bool Success, string? Message)> ChatEkleAsync(int talepId, int kullaniciId, string mesaj);

    // Atama
    Task<List<TalepAtamaViewModel>> GetAtamalarAsync(int talepId);
    Task<(bool Success, string? Message)> KullaniciAtaAsync(int talepId, int atayanId, int atananId, string? aciklama);

    // Sevk
    Task<List<TalepSevkViewModel>> GetSevklerAsync(int talepId);
    Task<(bool Success, string? Message)> SevketAsync(int talepId, int sevkEdenId, int? hedefKullaniciId, int? hedefRolId, string? aciklama);

    // Ekler
    Task<List<TalepEkViewModel>> GetEklerAsync(int talepId);
    Task<(bool Success, string? Message)> EkEkleAsync(int talepId, int? yanitId, int kullaniciId, IFormFile dosya);
    Task<(bool Success, string? Message)> EkSilAsync(int ekId, int kullaniciId);
    Task<(bool Success, byte[]? Data, string? DosyaAdi, string? ContentType)> EkIndirAsync(int ekId);

    // Kategoriler
    Task<List<TalepKategoriViewModel>> GetKategorilerAsync();
}

