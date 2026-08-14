using Microsoft.EntityFrameworkCore;
using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
// DbSet properties are initialized by EF Core at runtime. Suppress CS8618 rather than using
// null-forgiving operators, which are unnecessary here and flagged by SonarQube.
#pragma warning disable CS8618
    public DbSet<CaseMetadata> CaseMetadata { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<CaseActiveManageMaterialsOperation> CaseActiveManageMaterialsOperations { get; set; }
#pragma warning restore CS8618

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var addedEntries = ChangeTracker.Entries<IAuditableCreated>();
        foreach (var entry in addedEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        var updatedEntries = ChangeTracker.Entries<IAuditableUpdated>();
        foreach (var entry in updatedEntries)
        {
            if (entry.Entity.Id != Guid.Empty && entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
