using Microsoft.EntityFrameworkCore;
using TestService.Domain.Test;
using TestService.Infrastructure.Persistence.Configurations;

namespace TestService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new TestConfiguration());

        // Configure domain events (ignore them in database)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var domainEventsProperty = entityType.ClrType.GetProperty("DomainEvents");
            if (domainEventsProperty != null)
            {
                modelBuilder.Entity(entityType.ClrType).Ignore("DomainEvents");
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedAt = DateTime.UtcNow;
            }
            
            if (entry.State == EntityState.Modified)
            {
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}