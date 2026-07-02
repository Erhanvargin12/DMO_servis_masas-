using Core.Entities;

namespace Entities.Concrete;

public class Branch : IEntity
{
    public int Id { get; set; }
    public string DepartmentId { get; set; }           // Kaynak sistemdeki ID (string)
    public string? ParentDepartmentId { get; set; }     // Üst birimin kaynak ID'si
    public string Name { get; set; }                   // Şube/Müdürlük adı
    public string? Description { get; set; }           // Açıklama (isteğe bağlı)
}
