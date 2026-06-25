using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Entities.DTOs;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Concrete;

[SecuredOperation("Admin")]
public class MenuOperationClaimManager : IMenuOperationClaimService
{
    private readonly IMenuOperationClaimDal _menuOperationClaimDal;
    private readonly IMenuDal _menuDal;
    private readonly IOperationClaimDal _operationClaimDal;

    public MenuOperationClaimManager(IMenuOperationClaimDal menuOperationClaimDal,IMenuDal menuDal,IOperationClaimDal operationClaimDal)
    {
        _menuOperationClaimDal = menuOperationClaimDal;
        _menuDal = menuDal;
        _operationClaimDal = operationClaimDal;
    }


    public async Task<IDataResult<List<MenuOperationClaim>>> GetAllAsync(Expression<Func<MenuOperationClaim, bool>> filter = null)
    {
        return new SuccessDataResult<List<MenuOperationClaim>>(await _menuOperationClaimDal.GetAllAsync(filter),Messages.MenuOperationClaimsListed);
    }
    public async Task<IDataResult<List<MenuOperationClaim>>> GetByMenuIdAsync(int menuId)
    {
        return new SuccessDataResult<List<MenuOperationClaim>>(await _menuOperationClaimDal.GetAllAsync(m => m.MenuId == menuId),Messages.MenuOperationClaimsListed);
    }
    public async Task<IResult> AddAsync(MenuOperationClaim menuOperationClaim)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfMenuExistsAsync(menuOperationClaim.MenuId),
                                           () => CheckIfOperationClaimExistsAsync(menuOperationClaim.OperationClaimId),
                                           () => CheckIfMenuOperationClaimAlreadyExistsAsync(menuOperationClaim.MenuId, menuOperationClaim.OperationClaimId));

        if (result != null)
        {
            return result;
        }

        await _menuOperationClaimDal.AddAsync(menuOperationClaim);
        return new SuccessResult(Messages.MenuOperationClaimAdded);
    }
    public async Task<IResult> DeleteAsync(MenuOperationClaim menuOperationClaim)
    {
        IResult result = await BusinessRules.RunAsync(() => CheckIfMenuOperationClaimExistsAsync(menuOperationClaim.Id));

        if (result != null)
        {
            return result;
        }

        var existingClaim = await _menuOperationClaimDal.GetAsync(m => m.Id == menuOperationClaim.Id);
        await _menuOperationClaimDal.DeleteAsync(existingClaim);
        return new SuccessResult(Messages.MenuOperationClaimDeleted);
    }
    public async Task<IResult> UpdateAsync(MenuOperationClaimUpdateDto menuOperationClaimUpdateDto)
    {
        var claimsResult = await GetByMenuIdAsync(menuOperationClaimUpdateDto.MenuId);
        var claims = claimsResult.Data;
        var currentIds = claims.Select(x => x.OperationClaimId).ToHashSet();

        foreach (var id in menuOperationClaimUpdateDto.RoleIds.Except(currentIds))
        {
            var result = await AddAsync(new MenuOperationClaim { MenuId = menuOperationClaimUpdateDto.MenuId, OperationClaimId = id });
            if (!result.Success)
                return new ErrorResult(Messages.MenuOperationClaimNotFound);
        }
            
        foreach (var claim in claims.Where(x => !menuOperationClaimUpdateDto.RoleIds.Contains(x.OperationClaimId)))
        {
            var result = await DeleteAsync(claim);
            if (!result.Success)
                return new ErrorResult(Messages.MenuOperationClaimNotFound);
        }

        return new SuccessResult(Messages.OperationClaimAdded);
    }
    public async Task<IResult> DeleteByMenuIdAsync(int menuId)
    {
        var claims = await _menuOperationClaimDal.GetAllAsync(m => m.MenuId == menuId);
        foreach (var claim in claims)
        {
            await _menuOperationClaimDal.DeleteAsync(claim);
        }
        return new SuccessResult(Messages.MenuOperationClaimsDeleted);
    }


    #region Business Rules
    private async Task<IResult> CheckIfMenuExistsAsync(int menuId)
    {
        var menu = await _menuDal.GetAsync(m => m.Id == menuId);
        if (menu == null)
        {
            return new ErrorResult(Messages.MenuNotFound);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfOperationClaimExistsAsync(int operationClaimId)
    {
        var claim = await _operationClaimDal.GetAsync(o => o.Id == operationClaimId);
        if (claim == null)
        {
            return new ErrorResult(Messages.OperationClaimNotFound);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfMenuOperationClaimAlreadyExistsAsync(int menuId, int operationClaimId)
    {
        var claims = await _menuOperationClaimDal.GetAllAsync(m =>
            m.MenuId == menuId && m.OperationClaimId == operationClaimId);

        if (claims.Any())
        {
            return new ErrorResult(Messages.MenuOperationClaimAlreadyExists);
        }
        return new SuccessResult();
    }
    private async Task<IResult> CheckIfMenuOperationClaimExistsAsync(int id)
    {
        var claim = await _menuOperationClaimDal.GetAsync(m => m.Id == id);
        if (claim == null)
        {
            return new ErrorResult(Messages.MenuOperationClaimNotFound);
        }
        return new SuccessResult();
    }
    #endregion
}
