using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Abstract;

public interface IMenuDal : IEntityRepository<Menu>
{
    List<MenuDto> GetMenusByUserId(int userId);
}

