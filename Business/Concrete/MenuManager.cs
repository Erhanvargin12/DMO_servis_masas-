using Business.Abstract;
using Business.BusinessAspects.Autofac;
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

    public MenuManager(IMenuDal menuDal)
    {
        _menuDal = menuDal;
    }

    [SecuredOperation("Admin")]
    public IDataResult<List<Menu>> GetAll(Expression<Func<Menu, bool>> filter = null)
    {
        return new SuccessDataResult<List<Menu>>(_menuDal.GetAll(filter), Messages.MenusListed);
    }

    [SecuredOperation("Admin")]
    public IDataResult<Menu> GetById(int id)
    {
        var result = _menuDal.Get(m => m.Id == id);
        if (result == null)
        {
            return new ErrorDataResult<Menu>(Messages.MenuNotFound);
        }
        return new SuccessDataResult<Menu>(result);
    }

    // Bu metod yetkisiz kullanıcılar için de çalışmalı (menü gösterimi için)
    public IDataResult<List<MenuDto>> GetMenusByUserId(int userId)
    {
        var menus = _menuDal.GetMenusByUserId(userId);
        return new SuccessDataResult<List<MenuDto>>(menus, Messages.MenusListed);
    }

    [SecuredOperation("Admin")]
    public IResult Add(Menu menu)
    {
        IResult result = BusinessRules.Run(
            CheckIfMenuNameExists(menu.Name, menu.ParentId)
        );

        if (result != null)
        {
            return result;
        }

        menu.CreateAt = DateTime.Now;
        _menuDal.Add(menu);
        return new SuccessResult(Messages.MenuAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Update(Menu menu)
    {
        IResult result = BusinessRules.Run(
            CheckIfMenuExists(menu.Id),
            CheckIfMenuNameExistsForUpdate(menu.Id, menu.Name, menu.ParentId)
        );

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

    [SecuredOperation("Admin")]
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

    #endregion
}
