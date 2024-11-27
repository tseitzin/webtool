using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public required DbSet<User> Users { get; set; }
    public required DbSet<AuditLog> AuditLogs { get; set; }
    public required DbSet<StockData> StockData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users"); // PostgreSQL convention: lowercase table names
            
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

            // Configure column names to follow PostgreSQL conventions
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.Name).HasColumnName("name");
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
            entity.Property(u => u.IsAdmin).HasColumnName("is_admin");
            entity.Property(u => u.ResetToken).HasColumnName("reset_token");
            entity.Property(u => u.ResetTokenExpiry).HasColumnName("reset_token_expiry");
            entity.Property(u => u.CreatedDate).HasColumnName("created_date");
            entity.Property(u => u.LastLoginDate).HasColumnName("last_login_date");
            entity.Property(u => u.FailedLogins).HasColumnName("failed_logins");
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs"); // PostgreSQL convention: lowercase table names
            
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

            // Configure column names to follow PostgreSQL conventions
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.Event).HasColumnName("event");
            entity.Property(a => a.Email).HasColumnName("email");
            entity.Property(a => a.Success).HasColumnName("success");
            entity.Property(a => a.FailureReason).HasColumnName("failure_reason");
            entity.Property(a => a.IpAddress).HasColumnName("ip_address");
            entity.Property(a => a.Timestamp).HasColumnName("timestamp");
        });

        // Configure StockData entity
        modelBuilder.Entity<StockData>(entity =>
        {
            entity.ToTable("stock_data"); // PostgreSQL convention: lowercase table names

            entity.HasIndex(s => s.Symbol);
            entity.HasIndex(s => s.Timestamp);

            entity.Property(s => s.Symbol)
                  .HasMaxLength(10)
                  .IsRequired();

            // Configure column names to follow PostgreSQL conventions
            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.Symbol).HasColumnName("symbol");
            entity.Property(s => s.Price).HasColumnName("price");
            entity.Property(s => s.Change).HasColumnName("change");
            entity.Property(s => s.ChangePercent).HasColumnName("change_percent");
            entity.Property(s => s.Volume).HasColumnName("volume");
            entity.Property(s => s.MarketCap).HasColumnName("market_cap");
            entity.Property(s => s.Timestamp)
                  .HasColumnName("timestamp")
                  .HasColumnType("timestamp with time zone");
            entity.Property(s => s.Open).HasColumnName("open");
            entity.Property(s => s.High).HasColumnName("high");
            entity.Property(s => s.Low).HasColumnName("low");
            entity.Property(s => s.PreviousClose).HasColumnName("previous_close");
        });
    }
}