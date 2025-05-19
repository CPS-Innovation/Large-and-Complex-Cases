using Microsoft.EntityFrameworkCore;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Configurations;

namespace CPS.ComplexCases.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
  public DbSet<CaseMetadata> CaseMetadata { get; set; }
  public DbSet<AuditLog> AuditLogs { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditLogConfiguration).Assembly);
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
