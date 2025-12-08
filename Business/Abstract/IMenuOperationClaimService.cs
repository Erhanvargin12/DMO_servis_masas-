using Core.Utilities.Results;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IMenuOperationClaimService
{
    IDataResult<List<MenuOperationClaim>> GetAll(Expression<Func<MenuOperationClaim, bool>> filter = null);
    IDataResult<List<MenuOperationClaim>> GetByMenuId(int menuId);
    IResult Add(MenuOperationClaim menuOperationClaim);
    IResult Delete(MenuOperationClaim menuOperationClaim);
    IResult DeleteByMenuId(int menuId);
}

