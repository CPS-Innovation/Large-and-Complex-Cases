using Microsoft.EntityFrameworkCore;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Configurations;

namespace CPS.ComplexCases.Data;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
  public DbSet<CaseMetadata> CaseMetadata { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    CaseMetadataConfiguration.Configure(modelBuilder);
  }
}
