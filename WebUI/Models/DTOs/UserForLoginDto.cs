using Core.Entities;

namespace WebUI.Models.DTOs;

public class UserForLoginDto : IDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
