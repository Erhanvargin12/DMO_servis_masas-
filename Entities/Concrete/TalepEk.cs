using Core.Entities;

namespace Entities.Concrete;

public class TalepEk : IEntity
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int? YanitId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;
    public long DosyaBoyutu { get; set; }
    public string DosyaTipi { get; set; } = string.Empty;
    public int YukleyenKullaniciId { get; set; }
    public DateTime YuklemeTarihi { get; set; }
}

