using CorporateSystem.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Auth.Infrastructure;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    public DataContext()
    {
    }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Role).IsRequired();
            entity.Property(u => u.FirstName).IsRequired();
            entity.Property(u => u.LastName).IsRequired();
            entity.Property(u => u.Gender).IsRequired();
            
            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(u => u.Gender).HasDatabaseName("IX_Users_Gender");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired();
            entity.Property(rt => rt.IpAddress).IsRequired();
            entity.Property(rt => rt.ExpiryOn).IsRequired();

            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("IX_RefreshToken_Token");
        });

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Payload).IsRequired();
            entity.Property(x => x.EventType).IsRequired().HasMaxLength(255);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.Processed).HasDefaultValue(false);

            entity.HasIndex(x => new { x.Processed, x.CreatedAtUtc })
                .HasDatabaseName("IX_OutboxEvent_Processed_CreatedAt");
        });
    }
}