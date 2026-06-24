using Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Concrete;

public class User : IEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt  { get; set; }
    public bool Status { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public string RegistirationId { get; set; }
    public string OrganizationUnitId { get; set; }
    public int PositionId { get; set; }

    [NotMapped]
    public string? AuthenticationProviderType { get; set; }
}
