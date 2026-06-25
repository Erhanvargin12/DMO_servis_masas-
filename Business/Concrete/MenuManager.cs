using Business.Abstract;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System.Linq.Expressions;

namespace Business.Concrete;


public class MenuManager : IMenuService
{
    private readonly IMenuDal _menuDal;
    private readonly IMenuOperationClaimDal _menuOperationClaimDal;
    private readonly IOperationClaimDal _operationClaimDal;
    private readonly IUserOperationClaimDal _userOperationClaimDal;

    public MenuManager(IMenuDal menuDal, IMenuOperationClaimDal menuOperationClaimDal, IOperationClaimDal operationClaimDal, IUserOperationClaimDal userOperationClaimDal)
    {
        _menuDal = menuDal;
        _menuOperationClaimDal = menuOperationClaimDal;
        _operationClaimDal = operationClaimDal;
        _userOperationClaimDal = userOperationClaimDal;

    }

    public async Task<IDataResult<List<Menu>>> GetAllAsync(Expression<Func<Menu, bool>> filter = null)
    {
        return new SuccessDataResult<List<Menu>>(await _menuDal.GetAllAsync(filter), Messages.MenusListed);
    }
    public async Task<IDataResult<Menu>> GetByIdAsync(int id)
    {
        var result = await _menuDal.GetAsync(m => m.Id == id);
        if (result == null)
        {
            return new ErrorDataResult<Menu>(Messages.MenuNotFound);
        }
        return new SuccessDataResult<Menu>(result);
    }
    public async Task<IDataResult<List<MenuDto>>> GetMenuDetailsAsync()
    {
        var menus = await _menuDal.GetAllAsync();
        var menuClaims = await _menuOperationClaimDal.GetAllAsync();
        var operationClaims = await _operationClaimDal.GetAllAsync();

        var dto = new List<MenuDto>();

        foreach(var menu in menus.Where(m => m.ParentId == null).OrderBy(x => x.DisplayOrder))
        {
            dto.Add(new MenuDto
            {
                Parent = menu,
                Childrens = await GetChildrenAndClaimsByParentIdAsync(menu.Id),
                Claims = await _menuOperationClaimDal.GetAllAsync(c => c.MenuId == menu.Id)
            });
        }

        return new SuccessDataResult<List<MenuDto>>(dto, Messages.MenusListed);
    }
    public async Task<IDataResult<List<MenuDto>>> GetMenusByUserIdAsync(int userId)
    {

        var userClaims = await _userOperationClaimDal.GetAllAsync(c => c.UserId == userId);
        var menus = await _menuDal.GetAllAsync();
        var menuClaims = await _menuOperationClaimDal.GetAllAsync();

        var userMenus = userClaims.SelectMany(uc => menus
                                  .Where(m => menuClaims
                                  .Any(mc => mc.OperationClaimId == uc.OperationClaimId && mc.MenuId == m.Id)))
                                  .Distinct()
                                  .ToList();

        var dto = new List<MenuDto>();
        foreach(var userMenu in userMenus.Where(um => um.ParentId == null).OrderBy(x => x.DisplayOrder))
        {
            dto.Add(new MenuDto
            {
                Parent = userMenu, 
                Childrens = await GetChildrenAndClaimsByParentIdAsync(userMenu.Id),
                Claims = menuClaims.Where(c => c.MenuId == userMenu.Id).ToList()
            });
        }


        return new SuccessDataResult<List<MenuDto>>(dto, Messages.MenusListed);
    }

    public async Task<IResult> AddAsync(Menu menu)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfMenuNameExistsAsync(menu.Name, menu.ParentId));

        if (result != null)
        {
            return result;
        }

        menu.CreateAt = DateTime.Now;
        await _menuDal.AddAsync(menu);
        return new SuccessResult(Messages.MenuAdded);
    }
    public async Task<IResult> UpdateAsync(Menu menu)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfMenuExistsAsync(menu.Id), () => CheckIfMenuNameExistsForUpdateAsync(menu.Id, menu.Name, menu.ParentId));

        if (result != null)
        {
            return result;
        }

        var existingMenu = await _menuDal.GetAsync(m => m.Id == menu.Id);
        existingMenu.Name = menu.Name;
        existingMenu.Description = menu.Description;
        existingMenu.Icon = menu.Icon;
        existingMenu.Url = menu.Url;
        existingMenu.ParentId = menu.ParentId;
        existingMenu.DisplayOrder = menu.DisplayOrder;
        existingMenu.IsActive = menu.IsActive;
        existingMenu.UpdateAt = DateTime.Now;

        await _menuDal.UpdateAsync(existingMenu);
        return new SuccessResult(Messages.MenuUpdated);
    }
    public async Task<IResult> DeleteAsync(Menu menu)
    {
        IResult result = await BusinessRules.RunAsync(
            () => CheckIfMenuExistsAsync(menu.Id),
            () => CheckIfMenuHasChildrenAsync(menu.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingMenu = await _menuDal.GetAsync(m => m.Id == menu.Id);
        await _menuDal.DeleteAsync(existingMenu);
        return new SuccessResult(Messages.MenuDeleted);
    }


    #region Business Rules
    private async Task<IResult> CheckIfMenuNameExistsAsync(string name, int? parentId)
    {
        var menus = await _menuDal.GetAllAsync(m => m.Name == name && m.ParentId == parentId);
        if (menus.Any())
        {
            return new ErrorResult(Messages.MenuAlreadyExists);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfMenuNameExistsForUpdateAsync(int id, string name, int? parentId)
    {
        var menus = await _menuDal.GetAllAsync(m => m.Name == name && m.ParentId == parentId && m.Id != id);
        if (menus.Any())
        {
            return new ErrorResult(Messages.MenuAlreadyExists);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfMenuExistsAsync(int id)
    {
        var menu = await _menuDal.GetAsync(m => m.Id == id);
        if (menu == null)
        {
            return new ErrorResult(Messages.MenuNotFound);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfMenuHasChildrenAsync(int id)
    {
        var childrens = await _menuDal.GetAllAsync(m => m.ParentId == id);
        if (childrens.Any())
        {
            return new ErrorResult(Messages.MenuHasChildren);
        }
        return new SuccessResult();
    }
    private async Task<List<MenuChildrenDto>> GetChildrenAndClaimsByParentIdAsync(int menuId)
    {
        var childrens = await _menuDal.GetAllAsync(c => c.ParentId == menuId);
        var dto = new List<MenuChildrenDto>();
        foreach(var child in childrens)
        {
            dto.Add(new MenuChildrenDto
            {
                Children = child,
                Claims = await _menuOperationClaimDal.GetAllAsync(c => c.MenuId == child.Id)
            });
        }
        return dto;
    }
    #endregion
}
