using CorporateSystem.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Infrastructure.Repositories;

public class DataContext : DbContext
{
    public DbSet<EmailMessage> EmailMessages { get; set; }
    
    public DataContext()
    {
    }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiverEmail).IsRequired();
            entity.Property(e => e.SenderEmail).IsRequired();
            entity.Property(e => e.Message).IsRequired();

            entity.HasIndex(e => e.ReceiverEmail);
            entity.HasIndex(e => e.SenderEmail);
        });
    }
}