using Core.Utilities.Results;
using Entities.Concrete;

namespace Business.Abstract;

public interface ITalepService
{
    // ── Talep CRUD ─────────────────────────────────────────────────────────
    IDataResult<List<Talep>> GetAll(int? olusturanId = null, int? atananId = null, TalepDurum? durum = null);
    IDataResult<Talep> GetById(int id);
    IResult TalepOlustur(Talep talep);
    IResult TalepGuncelle(Talep talep);
    IResult TalepSil(int id);
    IResult TalepKapat(int id, int kullaniciId);
    IResult TalepIptal(int id, int kullaniciId);
    IResult DurumGuncelle(int id, TalepDurum durum, int kullaniciId);

    // ── Yanıt ──────────────────────────────────────────────────────────────
    IDataResult<List<TalepYanit>> GetYanitlar(int talepId);
    IResult YanitEkle(TalepYanit yanit);
    IResult YanitSil(int yanitId, int kullaniciId);

    // ── Chat ───────────────────────────────────────────────────────────────
    IDataResult<List<TalepChat>> GetChatMesajlari(int talepId);
    IResult ChatMesajEkle(TalepChat chat);

    // ── Atama ──────────────────────────────────────────────────────────────
    IDataResult<List<TalepAtama>> GetAtamalar(int talepId);
    IResult KullaniciAta(TalepAtama atama);

    // ── Sevk ───────────────────────────────────────────────────────────────
    IDataResult<List<TalepSevk>> GetSevkler(int talepId);
    IResult TalepSevket(TalepSevk sevk);

    // ── Ek (Dosya) ─────────────────────────────────────────────────────────
    IDataResult<List<TalepEk>> GetEkler(int talepId);
    IDataResult<TalepEk> GetEkById(int ekId);
    IResult EkEkle(TalepEk ek);
    IResult EkSil(int ekId, int kullaniciId);

    // ── Kategori ───────────────────────────────────────────────────────────
    IDataResult<List<TalepKategori>> GetKategoriler();
    IResult KategoriEkle(TalepKategori kategori);
    IResult KategoriGuncelle(TalepKategori kategori);
    IResult KategoriSil(int id);
}

