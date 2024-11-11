using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email)
                  .IsUnique();

            entity.Property(u => u.Email)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(u => u.Name)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(u => u.PasswordHash)
                  .IsRequired();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.Event)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(a => a.Email)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(a => a.IpAddress)
                  .HasMaxLength(45);

            entity.Property(a => a.FailureReason)
                  .HasMaxLength(256);
        });
    }
}