using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework;

public class EfMenuDal : EfEntityRepositoryBase<Menu, MsSqlDbContext>, IMenuDal
{
    private readonly MsSqlDbContext _context;

    public EfMenuDal(MsSqlDbContext context) : base(context)
    {
        _context = context;
    }

    public List<MenuDto> GetMenusByUserId(int userId)
    {
        // Kullanıcının sahip olduğu yetkileri al
        var userClaimIds = _context.UserOperationClaims
            .Where(uoc => uoc.UserId == userId)
            .Select(uoc => uoc.OperationClaimId)
            .ToList();

        // Bu yetkilere sahip menüleri al
        var menuIds = _context.MenuOperationClaims
            .Where(moc => userClaimIds.Contains(moc.OperationClaimId))
            .Select(moc => moc.MenuId)
            .Distinct()
            .ToList();

        // Aktif menüleri getir (hem yetkili olanlar hem de parent'ları)
        var allMenus = _context.Menus
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToList();

        // Yetkili menüleri filtrele
        var authorizedMenus = allMenus
            .Where(m => menuIds.Contains(m.Id) || m.ParentId == null)
            .ToList();

        // Hiyerarşik yapıyı oluştur
        return BuildMenuHierarchy(authorizedMenus, menuIds, null);
    }

    private List<MenuDto> BuildMenuHierarchy(List<Menu> allMenus, List<int> authorizedMenuIds, int? parentId)
    {
        var result = new List<MenuDto>();

        var menus = allMenus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.DisplayOrder)
            .ToList();

        foreach (var menu in menus)
        {
            var children = BuildMenuHierarchy(allMenus, authorizedMenuIds, menu.Id);
            
            // Sadece yetkili menüleri veya yetkili çocuğu olan parent'ları ekle
            if (authorizedMenuIds.Contains(menu.Id) || children.Any())
            {
                result.Add(new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Description = menu.Description,
                    Icon = menu.Icon,
                    Url = menu.Url,
                    ParentId = menu.ParentId,
                    DisplayOrder = menu.DisplayOrder,
                    IsActive = menu.IsActive,
                    Children = children
                });
            }
        }

        return result;
    }
}

