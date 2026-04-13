using Entities.Concrete;
using WebUI.Models.DTOs;

namespace WebUI.Models.ViewModels;

public class MenuViewModel
{
    public List<MenuDto> Menu { get; set; }
    public List<Menu> Orphans { get; set; }
    public List<OperationClaim> Roles { get; set; }
}
