using Core.Entities;

namespace Entities.DTOs;

public class UserAddDto : IDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = "123456"; // Varsayılan şifre
    public bool Status { get; set; } = true;
    public string RegistirationId { get; set; }
    public string OrganizationUnitId { get; set; }
    public int PositionId { get; set; }
}

