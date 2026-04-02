using Core.Entities;
using Entities.Concrete;

namespace Entities.DTOs;

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