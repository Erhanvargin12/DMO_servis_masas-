using Business.Abstract;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Concrete;

public class TalepManager : ITalepService
{
    private readonly ITalepDal _talepDal;
    private readonly ITalepKategoriDal _kategoriDal;
    private readonly ITalepYanitDal _yanitDal;
    private readonly ITalepChatDal _chatDal;
    private readonly ITalepAtamaDal _atamaDal;
    private readonly ITalepSevkDal _sevkDal;
    private readonly ITalepEkDal _ekDal;

    public TalepManager(
        ITalepDal talepDal,
        ITalepKategoriDal kategoriDal,
        ITalepYanitDal yanitDal,
        ITalepChatDal chatDal,
        ITalepAtamaDal atamaDal,
        ITalepSevkDal sevkDal,
        ITalepEkDal ekDal)
    {
        _talepDal = talepDal;
        _kategoriDal = kategoriDal;
        _yanitDal = yanitDal;
        _chatDal = chatDal;
        _atamaDal = atamaDal;
        _sevkDal = sevkDal;
        _ekDal = ekDal;
    }

    // ── Talep CRUD ─────────────────────────────────────────────────────────

    public IDataResult<List<Talep>> GetAll(int? olusturanId = null, int? atananId = null, TalepDurum? durum = null)
    {
        var talepler = _talepDal.GetAll();

        if (olusturanId.HasValue)
            talepler = talepler.Where(t => t.OlusturanKullaniciId == olusturanId.Value).ToList();

        if (atananId.HasValue)
            talepler = talepler.Where(t => t.AtananKullaniciId == atananId.Value).ToList();

        if (durum.HasValue)
            talepler = talepler.Where(t => t.Durum == durum.Value).ToList();

        return new SuccessDataResult<List<Talep>>(
            talepler.OrderByDescending(t => t.OlusturmaTarihi).ToList(),
            Messages.TaleplerListelendi);
    }

    public IDataResult<Talep> GetById(int id)
    {
        var talep = _talepDal.Get(t => t.Id == id);
        if (talep == null)
            return new ErrorDataResult<Talep>(Messages.TalepBulunamadi);

        return new SuccessDataResult<Talep>(talep);
    }

    public IResult TalepOlustur(Talep talep)
    {
        IResult result = BusinessRules.Run(
            CheckIfKategoriExists(talep.KategoriId),
            CheckIfBaslikValid(talep.Baslik)
        );
        if (result != null) return result;

        talep.OlusturmaTarihi = DateTime.Now;
        talep.Durum = TalepDurum.Acik;
        _talepDal.Add(talep);
        return new SuccessResult(Messages.TalepOlusturuldu);
    }

    public IResult TalepGuncelle(Talep talep)
    {
        var existing = _talepDal.Get(t => t.Id == talep.Id);
        if (existing == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        existing.Baslik = talep.Baslik;
        existing.Aciklama = talep.Aciklama;
        existing.KategoriId = talep.KategoriId;
        existing.Oncelik = talep.Oncelik;
        existing.GuncellenmeTarihi = DateTime.Now;
        _talepDal.Update(existing);
        return new SuccessResult(Messages.TalepGuncellendi);
    }

    public IResult TalepSil(int id)
    {
        var talep = _talepDal.Get(t => t.Id == id);
        if (talep == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        _talepDal.Delete(talep);
        return new SuccessResult(Messages.TalepSilindi);
    }

    public IResult TalepKapat(int id, int kullaniciId)
        => DurumGuncelle(id, TalepDurum.Kapali, kullaniciId);

    public IResult TalepIptal(int id, int kullaniciId)
        => DurumGuncelle(id, TalepDurum.Iptal, kullaniciId);

    public IResult DurumGuncelle(int id, TalepDurum durum, int kullaniciId)
    {
        var talep = _talepDal.Get(t => t.Id == id);
        if (talep == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        talep.Durum = durum;
        talep.GuncellenmeTarihi = DateTime.Now;
        if (durum == TalepDurum.Kapali || durum == TalepDurum.Iptal)
            talep.KapanmaTarihi = DateTime.Now;

        _talepDal.Update(talep);
        return new SuccessResult(Messages.TalepDurumuGuncellendi);
    }

    // ── Yanıt ──────────────────────────────────────────────────────────────

    public IDataResult<List<TalepYanit>> GetYanitlar(int talepId)
    {
        var yanitlar = _yanitDal.GetAll(y => y.TalepId == talepId)
                                .OrderBy(y => y.OlusturmaTarihi)
                                .ToList();
        return new SuccessDataResult<List<TalepYanit>>(yanitlar);
    }

    public IResult YanitEkle(TalepYanit yanit)
    {
        if (string.IsNullOrWhiteSpace(yanit.Icerik))
            return new ErrorResult(Messages.YanitIcerikBos);

        var talep = _talepDal.Get(t => t.Id == yanit.TalepId);
        if (talep == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        yanit.OlusturmaTarihi = DateTime.Now;
        _yanitDal.Add(yanit);

        // Talep durumunu "İşleniyor"a çek (eğer açıksa)
        if (talep.Durum == TalepDurum.Acik)
        {
            talep.Durum = TalepDurum.Isleniyor;
            talep.GuncellenmeTarihi = DateTime.Now;
            _talepDal.Update(talep);
        }

        return new SuccessResult(Messages.YanitEklendi);
    }

    public IResult YanitSil(int yanitId, int kullaniciId)
    {
        var yanit = _yanitDal.Get(y => y.Id == yanitId);
        if (yanit == null)
            return new ErrorResult(Messages.YanitBulunamadi);

        _yanitDal.Delete(yanit);
        return new SuccessResult(Messages.YanitSilindi);
    }

    // ── Chat ───────────────────────────────────────────────────────────────

    public IDataResult<List<TalepChat>> GetChatMesajlari(int talepId)
    {
        var mesajlar = _chatDal.GetAll(c => c.TalepId == talepId)
                               .OrderBy(c => c.OlusturmaTarihi)
                               .ToList();
        return new SuccessDataResult<List<TalepChat>>(mesajlar);
    }

    public IResult ChatMesajEkle(TalepChat chat)
    {
        if (string.IsNullOrWhiteSpace(chat.Mesaj))
            return new ErrorResult(Messages.ChatMesajBos);

        chat.OlusturmaTarihi = DateTime.Now;
        _chatDal.Add(chat);
        return new SuccessResult(Messages.ChatMesajEklendi);
    }

    // ── Atama ──────────────────────────────────────────────────────────────

    public IDataResult<List<TalepAtama>> GetAtamalar(int talepId)
    {
        var atamalar = _atamaDal.GetAll(a => a.TalepId == talepId)
                                .OrderByDescending(a => a.AtamaTarihi)
                                .ToList();
        return new SuccessDataResult<List<TalepAtama>>(atamalar);
    }

    public IResult KullaniciAta(TalepAtama atama)
    {
        var talep = _talepDal.Get(t => t.Id == atama.TalepId);
        if (talep == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        atama.AtamaTarihi = DateTime.Now;
        _atamaDal.Add(atama);

        // Talep'in AtananKullaniciId'sini güncelle
        talep.AtananKullaniciId = atama.AtananKullaniciId;
        talep.GuncellenmeTarihi = DateTime.Now;
        _talepDal.Update(talep);

        return new SuccessResult(Messages.TalepAtandi);
    }

    // ── Sevk ───────────────────────────────────────────────────────────────

    public IDataResult<List<TalepSevk>> GetSevkler(int talepId)
    {
        var sevkler = _sevkDal.GetAll(s => s.TalepId == talepId)
                              .OrderByDescending(s => s.SevkTarihi)
                              .ToList();
        return new SuccessDataResult<List<TalepSevk>>(sevkler);
    }

    public IResult TalepSevket(TalepSevk sevk)
    {
        if (!sevk.HedefKullaniciId.HasValue && !sevk.HedefRolId.HasValue)
            return new ErrorResult(Messages.SevkHedefGerekli);

        var talep = _talepDal.Get(t => t.Id == sevk.TalepId);
        if (talep == null)
            return new ErrorResult(Messages.TalepBulunamadi);

        sevk.SevkTarihi = DateTime.Now;
        _sevkDal.Add(sevk);

        // Sevk edildiğinde atanan kullanıcıyı güncelle (hedef kullanıcıya)
        if (sevk.HedefKullaniciId.HasValue)
        {
            talep.AtananKullaniciId = sevk.HedefKullaniciId;
            talep.GuncellenmeTarihi = DateTime.Now;
            _talepDal.Update(talep);
        }

        return new SuccessResult(Messages.TalepSevkedildi);
    }

    // ── Ek (Dosya) ─────────────────────────────────────────────────────────

    public IDataResult<List<TalepEk>> GetEkler(int talepId)
    {
        var ekler = _ekDal.GetAll(e => e.TalepId == talepId)
                          .OrderByDescending(e => e.YuklemeTarihi)
                          .ToList();
        return new SuccessDataResult<List<TalepEk>>(ekler);
    }

    public IDataResult<TalepEk> GetEkById(int ekId)
    {
        var ek = _ekDal.Get(e => e.Id == ekId);
        if (ek == null)
            return new ErrorDataResult<TalepEk>(Messages.EkBulunamadi);

        return new SuccessDataResult<TalepEk>(ek);
    }

    public IResult EkEkle(TalepEk ek)
    {
        ek.YuklemeTarihi = DateTime.Now;
        _ekDal.Add(ek);
        return new SuccessResult(Messages.EkEklendi);
    }

    public IResult EkSil(int ekId, int kullaniciId)
    {
        var ek = _ekDal.Get(e => e.Id == ekId);
        if (ek == null)
            return new ErrorResult(Messages.EkBulunamadi);

        _ekDal.Delete(ek);
        return new SuccessResult(Messages.EkSilindi);
    }

    // ── Kategori ───────────────────────────────────────────────────────────

    public IDataResult<List<TalepKategori>> GetKategoriler()
    {
        var kategoriler = _kategoriDal.GetAll(k => k.AktifMi)
                                      .OrderBy(k => k.Ad)
                                      .ToList();
        return new SuccessDataResult<List<TalepKategori>>(kategoriler);
    }

    public IResult KategoriEkle(TalepKategori kategori)
    {
        kategori.OlusturmaTarihi = DateTime.Now;
        _kategoriDal.Add(kategori);
        return new SuccessResult(Messages.KategoriEklendi);
    }

    public IResult KategoriGuncelle(TalepKategori kategori)
    {
        var existing = _kategoriDal.Get(k => k.Id == kategori.Id);
        if (existing == null)
            return new ErrorResult(Messages.KategoriBulunamadi);

        existing.Ad = kategori.Ad;
        existing.Aciklama = kategori.Aciklama;
        existing.AktifMi = kategori.AktifMi;
        _kategoriDal.Update(existing);
        return new SuccessResult(Messages.KategoriGuncellendi);
    }

    public IResult KategoriSil(int id)
    {
        var kategori = _kategoriDal.Get(k => k.Id == id);
        if (kategori == null)
            return new ErrorResult(Messages.KategoriBulunamadi);

        kategori.AktifMi = false;
        _kategoriDal.Update(kategori);
        return new SuccessResult(Messages.KategoriSilindi);
    }

    // ── Business Rules ─────────────────────────────────────────────────────

    private IResult CheckIfKategoriExists(int kategoriId)
    {
        var k = _kategoriDal.Get(x => x.Id == kategoriId && x.AktifMi);
        if (k == null) return new ErrorResult(Messages.KategoriBulunamadi);
        return new SuccessResult();
    }

    private IResult CheckIfBaslikValid(string baslik)
    {
        if (string.IsNullOrWhiteSpace(baslik) || baslik.Length < 5)
            return new ErrorResult(Messages.TalepBaslikGecersiz);
        return new SuccessResult();
    }
}

