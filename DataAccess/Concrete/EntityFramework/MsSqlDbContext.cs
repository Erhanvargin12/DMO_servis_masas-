using Core.Entities.Concrete;
using Entities.Concrete;
using Entities.Concrete.Ticket;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework;

public class MsSqlDbContext : DbContext
{

    public MsSqlDbContext(DbContextOptions<MsSqlDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        string ticketSchema = "ticket";

        modelBuilder.Entity<Ticket>().ToTable("Tickets", ticketSchema);
        modelBuilder.Entity<Assignment>().ToTable("Assignments", ticketSchema);
        modelBuilder.Entity<Attachment>().ToTable("Attachments", ticketSchema);
        modelBuilder.Entity<Response>().ToTable("Responses", ticketSchema);
        modelBuilder.Entity<Transfer>().ToTable("Transfers", ticketSchema);

        //modelBuilder.Entity<Assignment>().HasOne(a => a.Ticket).WithOne(t => t.Assignment).HasForeignKey<Assignment>(a => a.TicketId);
        //modelBuilder.Entity<Response>().HasOne(r => r.Ticket).WithMany(t => t.Responses).HasForeignKey(r => r.TicketId);
        //modelBuilder.Entity<Attachment>().HasOne(a => a.Response).WithMany(r => r.Attachments).HasForeignKey(a => a.ResponseId);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuOperationClaim> MenuOperationClaims { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Ticket Modülü
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}
