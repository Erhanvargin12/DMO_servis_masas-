using Core.Entities;

namespace Entities.DTOs;

public class UserForRegisterDto : IDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
    public string RegistirationId { get; set; }
    public string BranchId { get; set; }
    public int PositionId { get; set; }

}
