using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Constants;

namespace CPS.ComplexCases.Data.Configurations;

public static class CaseMetadataConfiguration
{
  public static void Configure(ModelBuilder builder)
  {
    ConfigureEntities(builder);
  }

  private static void ConfigureEntities(ModelBuilder builder)
  {
    builder.Entity<CaseMetadata>(entity =>
    {
      entity.ToTable("case_metadata", SchemaNames.Lcc);

      entity.HasKey(x => x.CaseId);

      entity.Property(x => x.CaseId).HasColumnName("case_id").IsRequired();
      entity.Property(x => x.EgressWorkspaceId).HasColumnName("egress_workspace_id").HasMaxLength(200);
      entity.Property(x => x.NetappFolderPath).HasColumnName("netapp_folder_path").HasMaxLength(260);
    });
  }
}
