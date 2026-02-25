using Core.Entities;

namespace Entities.Concrete;

public class TalepYanit : IEntity
{
    public int Id { get; set; }
    public int TalepId { get; set; }
    public int KullaniciId { get; set; }
    public string Icerik { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; }
}

