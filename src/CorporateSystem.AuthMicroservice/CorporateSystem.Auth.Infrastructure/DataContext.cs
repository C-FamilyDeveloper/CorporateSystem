using CorporateSystem.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CorporateSystem.Auth.Infrastructure;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    public DbSet<Department> Departments { get; set; }

    public DataContext()
    {
    }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}