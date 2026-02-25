using Core.Entities;

namespace Entities.Concrete;

public class Talep : IEntity
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public int KategoriId { get; set; }
    public TalepOncelik Oncelik { get; set; } = TalepOncelik.Normal;
    public TalepDurum Durum { get; set; } = TalepDurum.Acik;
    public int OlusturanKullaniciId { get; set; }
    public int? AtananKullaniciId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
    public DateTime? KapanmaTarihi { get; set; }
}

