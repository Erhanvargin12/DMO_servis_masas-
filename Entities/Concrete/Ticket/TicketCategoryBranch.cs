using Core.Entities;

namespace Entities.Concrete.Ticket;

/// <summary>Kategori → Şube yönlendirme ilişkisi (çok-a-çok)</summary>
public class TicketCategoryBranch : IEntity
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int BranchId { get; set; }
    public int DisplayOrder { get; set; }
}
