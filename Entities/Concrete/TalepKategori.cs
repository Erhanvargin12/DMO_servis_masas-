using Core.Entities;

namespace Entities.Concrete;

public class TalepKategori : IEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; }
}

