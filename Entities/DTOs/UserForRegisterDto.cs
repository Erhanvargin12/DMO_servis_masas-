using Core.Entities;

namespace Entities.DTOs;

public class UserForRegisterDto : IDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
    public int RegistirationId { get; set; }
    public int BranchId { get; set; }
    public int PositionId { get; set; }

}
