using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System.Linq.Expressions;

namespace Business.Abstract;

public interface IMenuService
{
    IDataResult<List<Menu>> GetAll(Expression<Func<Menu, bool>> filter = null);
    IDataResult<Menu> GetById(int id);
    IDataResult<List<MenuDto>> GetMenuDetails();
    IDataResult<List<MenuDto>> GetMenusByUserId(int userId);
    IResult Add(Menu menu);
    IResult Update(Menu menu);
    IResult Delete(Menu menu);
}

