using Core.Entities;

namespace Entities.Concrete;

public class TalepSevk : IEntity
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int SevkEdenKullaniciId { get; set; }
    public int? HedefKullaniciId { get; set; }
    public int? HedefRolId { get; set; }
    public string? Aciklama { get; set; }
    public DateTime SevkTarihi { get; set; }
}

