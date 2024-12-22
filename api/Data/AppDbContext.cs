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
    public required DbSet<UserFavoriteStock> UserFavoriteStocks { get; set; }
    public required DbSet<UserOwnedStock> UserOwnedStocks { get; set; }
    public required DbSet<UserSavedStock> UserSavedStocks { get; set; }
    public required DbSet<UserSavedCrypto> UserSavedCryptos { get; set; }
    public required DbSet<MarketMover> MarketMovers { get; set; }

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

        // Configure UserFavoriteStock entity
        modelBuilder.Entity<UserFavoriteStock>(entity =>
        {
            entity.ToTable("user_favorite_stocks");

            entity.HasIndex(f => new { f.UserId, f.Symbol }).IsUnique();

            entity.Property(f => f.Symbol)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(f => f.Id).HasColumnName("id");
            entity.Property(f => f.UserId).HasColumnName("user_id");
            entity.Property(f => f.Symbol).HasColumnName("symbol");
            entity.Property(f => f.AddedAt)
                  .HasColumnName("added_at")
                  .HasColumnType("timestamp with time zone");

            entity.HasOne(f => f.User)
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserOwnedStock entity
        modelBuilder.Entity<UserOwnedStock>(entity =>
        {
            entity.ToTable("user_owned_stocks");

            entity.HasIndex(o => new { o.UserId, o.Symbol });

            entity.Property(o => o.Symbol)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(o => o.Notes)
                  .HasMaxLength(500);

            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.UserId).HasColumnName("user_id");
            entity.Property(o => o.Symbol).HasColumnName("symbol");
            entity.Property(o => o.Quantity).HasColumnName("quantity");
            entity.Property(o => o.PurchasePrice).HasColumnName("purchase_price");
            entity.Property(o => o.PurchaseDate)
                  .HasColumnName("purchase_date")
                  .HasColumnType("timestamp with time zone");
            entity.Property(o => o.Notes).HasColumnName("notes");

            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSavedStock>(entity =>
        {
            entity.ToTable("user_saved_stocks");

            entity.HasIndex(s => new { s.UserId, s.Symbol }).IsUnique();

            entity.Property(s => s.Symbol)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.UserId).HasColumnName("user_id");
            entity.Property(s => s.Symbol).HasColumnName("symbol");
            entity.Property(s => s.Price).HasColumnName("price");
            entity.Property(s => s.Change).HasColumnName("change");
            entity.Property(s => s.ChangePercent).HasColumnName("change_percent");
            entity.Property(s => s.Volume).HasColumnName("volume");
            entity.Property(s => s.MarketCap).HasColumnName("market_cap");
            entity.Property(s => s.SavedAt)
                  .HasColumnName("saved_at")
                  .HasColumnType("timestamp with time zone");
            entity.Property(s => s.Open).HasColumnName("open");
            entity.Property(s => s.High).HasColumnName("high");
            entity.Property(s => s.Low).HasColumnName("low");
            entity.Property(s => s.PreviousClose).HasColumnName("previous_close");

            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MarketMover>(entity =>
       {
           entity.ToTable("market_movers");
           
           entity.Property(m => m.Symbol)
                 .HasMaxLength(10)
                 .IsRequired();
           
           entity.Property(m => m.Type)
                 .HasMaxLength(10)
                 .IsRequired();

           entity.Property(m => m.LastUpdated)
                 .HasColumnType("timestamp with time zone");
       });

        modelBuilder.Entity<UserSavedCrypto>(entity =>
        {
            entity.ToTable("user_saved_cryptos");

            entity.HasIndex(c => new { c.UserId, c.Symbol }).IsUnique();

            entity.Property(c => c.Symbol)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.UserId).HasColumnName("user_id");
            entity.Property(c => c.Symbol).HasColumnName("symbol");
            entity.Property(c => c.Open).HasColumnName("open");
            entity.Property(c => c.Price).HasColumnName("price");
            entity.Property(c => c.Change).HasColumnName("change");
            entity.Property(c => c.ChangePercent).HasColumnName("change_percent");
            entity.Property(c => c.Volume).HasColumnName("volume");
            entity.Property(c => c.High24h).HasColumnName("high_24h");
            entity.Property(c => c.Low24h).HasColumnName("low_24h");
            entity.Property(c => c.SavedAt)
                  .HasColumnName("saved_at")
                  .HasColumnType("timestamp with time zone");

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
      }
}