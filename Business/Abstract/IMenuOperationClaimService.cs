using Core.Entities.DTOs;
using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IMenuOperationClaimService
{
    Task<IDataResult<List<MenuOperationClaim>>> GetAllAsync(Expression<Func<MenuOperationClaim, bool>> filter = null);
    Task<IDataResult<List<MenuOperationClaim>>> GetByMenuIdAsync(int menuId);
    Task<IResult> AddAsync(MenuOperationClaim menuOperationClaim);
    Task<IResult> UpdateAsync(MenuOperationClaimUpdateDto menuOperationClaimUpdateDto);
    Task<IResult> DeleteAsync(MenuOperationClaim menuOperationClaim);
    Task<IResult> DeleteByMenuIdAsync(int menuId);
}

