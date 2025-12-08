using Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework;

public class MsSqlDbContext: DbContext
{

    public MsSqlDbContext(DbContextOptions<MsSqlDbContext> options) : base(options)
    {
    }


    public DbSet<User> Users { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuOperationClaim> MenuOperationClaims { get; set; }
}
