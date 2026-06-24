using Core.Entities;

namespace Entities.DTOs;

public class UserUpdateDto : IDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string RegistirationId { get; set; }
    public string OrganizationUnitId { get; set; }
    public int PositionId { get; set; }
}

public class UserDeleteDto : IDto
{
    public int Id { get; set; }
}

public class ChangePasswordDto : IDto
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

