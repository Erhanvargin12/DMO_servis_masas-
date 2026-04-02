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

    public IDataResult<List<Menu>> GetAll(Expression<Func<Menu, bool>> filter = null)
    {
        return new SuccessDataResult<List<Menu>>(_menuDal.GetAll(filter), Messages.MenusListed);
    }
    public IDataResult<Menu> GetById(int id)
    {
        var result = _menuDal.Get(m => m.Id == id);
        if (result == null)
        {
            return new ErrorDataResult<Menu>(Messages.MenuNotFound);
        }
        return new SuccessDataResult<Menu>(result);
    }
    public IDataResult<List<MenuDto>> GetMenuDetails()
    {
        var menus = _menuDal.GetAll();
        var menuClaims = _menuOperationClaimDal.GetAll();
        var operationClaims = _operationClaimDal.GetAll();

        var dto = menus.Where(m => m.ParentId == null)
            .OrderBy(x=>x.DisplayOrder)
            .Select(menu => new MenuDto
            {
                Parent = menu,
                Childrens = GetChildrenAndClaimsByParentId(menu.Id),
                Claims = _menuOperationClaimDal.GetAll(c => c.MenuId == menu.Id)
            })
            .ToList();

        return new SuccessDataResult<List<MenuDto>>(dto, Messages.MenusListed);
    }
    public IDataResult<List<MenuDto>> GetMenusByUserId(int userId)
    {

        var userClaims = _userOperationClaimDal.GetAll(c => c.UserId == userId).ToList();
        var menus = _menuDal.GetAll();
        var menuClaims = _menuOperationClaimDal.GetAll();

        var userMenus = userClaims.SelectMany(uc => menus
                                  .Where(m => menuClaims
                                  .Any(mc => mc.OperationClaimId == uc.OperationClaimId && mc.MenuId == m.Id)))
                                  .Distinct()
                                  .ToList();

        var dto = userMenus.Where(um => um.ParentId == null)
                           .OrderBy(x => x.DisplayOrder)
                           .Select(userMenu => new MenuDto
                           {
                               Parent = userMenu, 
                               Childrens = GetChildrenAndClaimsByParentId(userMenu.Id),
                               Claims = menuClaims.Where(c => c.MenuId == userMenu.Id).ToList()
                           }).ToList();


        return new SuccessDataResult<List<MenuDto>>(dto, Messages.MenusListed);
    }

    public IResult Add(Menu menu)
    {
        IResult result = BusinessRules.Run(CheckIfMenuNameExists(menu.Name, menu.ParentId));

        if (result != null)
        {
            return result;
        }

        menu.CreateAt = DateTime.Now;
        _menuDal.Add(menu);
        return new SuccessResult(Messages.MenuAdded);
    }
    public IResult Update(Menu menu)
    {
        IResult result = BusinessRules.Run(CheckIfMenuExists(menu.Id), CheckIfMenuNameExistsForUpdate(menu.Id, menu.Name, menu.ParentId));

        if (result != null)
        {
            return result;
        }

        var existingMenu = _menuDal.Get(m => m.Id == menu.Id);
        existingMenu.Name = menu.Name;
        existingMenu.Description = menu.Description;
        existingMenu.Icon = menu.Icon;
        existingMenu.Url = menu.Url;
        existingMenu.ParentId = menu.ParentId;
        existingMenu.DisplayOrder = menu.DisplayOrder;
        existingMenu.IsActive = menu.IsActive;
        existingMenu.UpdateAt = DateTime.Now;

        _menuDal.Update(existingMenu);
        return new SuccessResult(Messages.MenuUpdated);
    }
    public IResult Delete(Menu menu)
    {
        IResult result = BusinessRules.Run(
            CheckIfMenuExists(menu.Id),
            CheckIfMenuHasChildren(menu.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingMenu = _menuDal.Get(m => m.Id == menu.Id);
        _menuDal.Delete(existingMenu);
        return new SuccessResult(Messages.MenuDeleted);
    }


    #region Business Rules
    private IResult CheckIfMenuNameExists(string name, int? parentId)
    {
        var exists = _menuDal.GetAll(m => m.Name == name && m.ParentId == parentId).Any();
        if (exists)
        {
            return new ErrorResult(Messages.MenuAlreadyExists);
        }
        return new SuccessResult();
    }
    private IResult CheckIfMenuNameExistsForUpdate(int id, string name, int? parentId)
    {
        var exists = _menuDal.GetAll(m => m.Name == name && m.ParentId == parentId && m.Id != id).Any();
        if (exists)
        {
            return new ErrorResult(Messages.MenuAlreadyExists);
        }
        return new SuccessResult();
    }
    private IResult CheckIfMenuExists(int id)
    {
        var menu = _menuDal.Get(m => m.Id == id);
        if (menu == null)
        {
            return new ErrorResult(Messages.MenuNotFound);
        }
        return new SuccessResult();
    }
    private IResult CheckIfMenuHasChildren(int id)
    {
        var hasChildren = _menuDal.GetAll(m => m.ParentId == id).Any();
        if (hasChildren)
        {
            return new ErrorResult(Messages.MenuHasChildren);
        }
        return new SuccessResult();
    }
    private List<MenuChildrenDto> GetChildrenAndClaimsByParentId(int menuId)
    {
        var childrens = _menuDal.GetAll(c => c.ParentId == menuId);
        var dto = childrens.Select(child => new MenuChildrenDto
        {
            Children = child,
            Claims = _menuOperationClaimDal.GetAll(c => c.MenuId == child.Id)
        }).ToList();
        return dto;
    }
    #endregion
}
