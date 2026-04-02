using Core.Entities;

namespace WebUI.Models.DTOs;

public class UserAddDto : IDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = "123456"; // Varsayılan şifre
    public bool Status { get; set; } = true;
    public int RegistirationId { get; set; }
    public int OrganizationUnitId { get; set; }
    public int PositionId { get; set; }
}

