using Core.Entities;

namespace Entities.DTOs;

public class UserAddDto : IDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = "123456"; // Varsayılan şifre
    public bool Status { get; set; } = true;
    public string EbysUserId { get; set; }
    public string BranchId { get; set; }
    public int PositionId { get; set; }
}

