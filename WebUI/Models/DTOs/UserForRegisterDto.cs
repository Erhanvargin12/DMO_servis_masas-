using Core.Entities;

namespace WebUI.Models.DTOs;

public class UserForRegisterDto : IDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
    public int EbysUserId { get; set; }
    public int BranchId { get; set; }
    public int PositionId { get; set; }

}
