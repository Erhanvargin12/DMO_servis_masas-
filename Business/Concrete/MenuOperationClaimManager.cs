using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Concrete;

public class MenuOperationClaimManager : IMenuOperationClaimService
{
    private readonly IMenuOperationClaimDal _menuOperationClaimDal;
    private readonly IMenuDal _menuDal;
    private readonly IOperationClaimDal _operationClaimDal;

    public MenuOperationClaimManager(
        IMenuOperationClaimDal menuOperationClaimDal,
        IMenuDal menuDal,
        IOperationClaimDal operationClaimDal)
    {
        _menuOperationClaimDal = menuOperationClaimDal;
        _menuDal = menuDal;
        _operationClaimDal = operationClaimDal;
    }

    [SecuredOperation("Admin")]
    public IDataResult<List<MenuOperationClaim>> GetAll(Expression<Func<MenuOperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<MenuOperationClaim>>(
            _menuOperationClaimDal.GetAll(filter),
            Messages.MenuOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public IDataResult<List<MenuOperationClaim>> GetByMenuId(int menuId)
    {
        return new SuccessDataResult<List<MenuOperationClaim>>(
            _menuOperationClaimDal.GetAll(m => m.MenuId == menuId),
            Messages.MenuOperationClaimsListed);
    }

    [SecuredOperation("Admin")]
    public IResult Add(MenuOperationClaim menuOperationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfMenuExists(menuOperationClaim.MenuId),
            CheckIfOperationClaimExists(menuOperationClaim.OperationClaimId),
            CheckIfMenuOperationClaimAlreadyExists(menuOperationClaim.MenuId, menuOperationClaim.OperationClaimId)
        );

        if (result != null)
        {
            return result;
        }

        _menuOperationClaimDal.Add(menuOperationClaim);
        return new SuccessResult(Messages.MenuOperationClaimAdded);
    }

    [SecuredOperation("Admin")]
    public IResult Delete(MenuOperationClaim menuOperationClaim)
    {
        IResult result = BusinessRules.Run(
            CheckIfMenuOperationClaimExists(menuOperationClaim.Id)
        );

        if (result != null)
        {
            return result;
        }

        var existingClaim = _menuOperationClaimDal.Get(m => m.Id == menuOperationClaim.Id);
        _menuOperationClaimDal.Delete(existingClaim);
        return new SuccessResult(Messages.MenuOperationClaimDeleted);
    }

    [SecuredOperation("Admin")]
    public IResult DeleteByMenuId(int menuId)
    {
        var claims = _menuOperationClaimDal.GetAll(m => m.MenuId == menuId);
        foreach (var claim in claims)
        {
            _menuOperationClaimDal.Delete(claim);
        }
        return new SuccessResult(Messages.MenuOperationClaimsDeleted);
    }

    #region Business Rules

    private IResult CheckIfMenuExists(int menuId)
    {
        var menu = _menuDal.Get(m => m.Id == menuId);
        if (menu == null)
        {
            return new ErrorResult(Messages.MenuNotFound);
        }
        return new SuccessResult();
    }

    private IResult CheckIfOperationClaimExists(int operationClaimId)
    {
        var claim = _operationClaimDal.Get(o => o.Id == operationClaimId);
        if (claim == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }

    private IResult CheckIfMenuOperationClaimAlreadyExists(int menuId, int operationClaimId)
    {
        var exists = _menuOperationClaimDal.GetAll(m =>
            m.MenuId == menuId && m.OperationClaimId == operationClaimId).Any();

        if (exists)
        {
            return new ErrorResult(Messages.MenuOperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }

    private IResult CheckIfMenuOperationClaimExists(int id)
    {
        var claim = _menuOperationClaimDal.Get(m => m.Id == id);
        if (claim == null)
        {
            return new ErrorResult(Messages.MenuOperationClaimNotFound);
        }
        return new SuccessResult();
    }

    #endregion
}
