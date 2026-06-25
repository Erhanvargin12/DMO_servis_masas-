using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IMenuService
{
    Task<IDataResult<List<Menu>>> GetAllAsync(Expression<Func<Menu, bool>> filter = null);
    Task<IDataResult<Menu>> GetByIdAsync(int id);
    Task<IDataResult<List<MenuDto>>> GetMenuDetailsAsync();
    Task<IDataResult<List<MenuDto>>> GetMenusByUserIdAsync(int userId);
    Task<IResult> AddAsync(Menu menu);
    Task<IResult> UpdateAsync(Menu menu);
    Task<IResult> DeleteAsync(Menu menu);
}

