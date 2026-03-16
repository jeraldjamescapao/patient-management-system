namespace PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PatientManagementSystem.Modules.Identity.Domain.Users;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}