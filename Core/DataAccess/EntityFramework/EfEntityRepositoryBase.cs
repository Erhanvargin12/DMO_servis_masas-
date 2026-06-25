using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core.DataAccess.EntityFramework;

public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
    where TEntity : class, IEntity, new() 
    where TContext : DbContext
{

    protected readonly TContext _context;
    public EfEntityRepositoryBase(TContext context)
    {
        _context = context;
    }


    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filter);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null)
    {
        return filter == null
            ? await _context.Set<TEntity>().AsNoTracking().ToListAsync()
            : await _context.Set<TEntity>().AsNoTracking().Where(filter).ToListAsync();
    }
}
