using Core.Entities;
using System.Linq.Expressions;

namespace Core.DataAccess;

public interface IEntityRepository<T> where T : class, IEntity, new()
{
    Task<T> GetAsync(Expression<Func<T, bool>> filter);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null);
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
    Task UpdateAsync(T entity);
}
