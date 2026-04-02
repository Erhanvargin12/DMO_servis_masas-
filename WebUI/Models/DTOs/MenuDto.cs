using Core.Entities;
using Entities.Concrete;

namespace WebUI.Models.DTOs;

public class MenuDto : IDto
{
    public Menu Parent { get; set; }
    public List<MenuChildrenDto> Childrens { get; set; } = new();
    public List<MenuOperationClaim> Claims { get; set; } = new();
}

public class MenuChildrenDto : IDto
{
    public Menu Children { get; set; } = new();
    public List<MenuOperationClaim> Claims { get; set; } = new();
}
public class MenuOperationClaimUpdateDto
{
    public int MenuId { get; set; }
    public List<int>? RoleIds { get; set; }
}
