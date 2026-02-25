using Core.Entities;

namespace Entities.Concrete;

public class TalepChat : IEntity
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int GonderenKullaniciId { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; }
}

