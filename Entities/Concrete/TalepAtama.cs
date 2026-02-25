using Core.Entities;

namespace Entities.Concrete;

public class TalepAtama : IEntity
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int AtayanKullaniciId { get; set; }
    public int AtananKullaniciId { get; set; }
    public string? Aciklama { get; set; }
    public DateTime AtamaTarihi { get; set; }
}

