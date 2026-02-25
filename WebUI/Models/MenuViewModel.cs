namespace WebUI.Models;

public class MenuViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<MenuViewModel> Children { get; set; } = new();

    // Sayfa yüklenirken atanan roller (modal için)
    public List<int> AssignedRoleIds { get; set; } = new();
}

/// <summary>
/// Menü listesi sayfası view modeli.
/// Menü ağacı client-side JS ile yönetildiğinden tüm kayıtlar gönderilir.
/// </summary>
public class MenuListViewModel
{
    public List<MenuViewModel> Menus { get; set; } = new();
    public List<RoleViewModel> Roles { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// Menüye rol atama sayfası view modeli.
/// Server-rendered, AJAX kullanılmaz.
/// </summary>
public class AssignMenuRolesViewModel
{
    public int MenuId { get; set; }
    public string MenuName { get; set; } = "";
    public string? MenuUrl { get; set; }
    public List<RoleViewModel> AllRoles { get; set; } = new();
    public List<int> SelectedRoleIds { get; set; } = new();
}
