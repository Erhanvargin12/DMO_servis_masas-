using Core.Entities.Concrete;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework;

public class MsSqlDbContext : DbContext
{

    public MsSqlDbContext(DbContextOptions<MsSqlDbContext> options) : base(options)
    {
    }


    public DbSet<User> Users { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuOperationClaim> MenuOperationClaims { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Talep Modülü
    public DbSet<TalepKategori> TalepKategoriler { get; set; }
    public DbSet<Talep> Talepler { get; set; }
    public DbSet<TalepYanit> TalepYanitlar { get; set; }
    public DbSet<TalepChat> TalepChatler { get; set; }
    public DbSet<TalepAtama> TalepAtamalar { get; set; }
    public DbSet<TalepSevk> TalepSevkler { get; set; }
    public DbSet<TalepEk> TalepEkler { get; set; }
}
