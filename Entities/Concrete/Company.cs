using Core.Entities;

namespace Entities.Concrete;

public class Company : IEntity
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string AuthorizedPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
