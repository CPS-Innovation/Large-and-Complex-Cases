using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Constants;

namespace CPS.ComplexCases.Data.Configurations;

public class CaseMetadataConfiguration : IEntityTypeConfiguration<CaseMetadata>
{
  public void Configure(EntityTypeBuilder<CaseMetadata> builder)
  {
    builder.ToTable("case_metadata", SchemaNames.Lcc);
    builder.HasKey(x => x.CaseId);

    builder.Property(x => x.CaseId).HasColumnName("case_id").IsRequired();
    builder.Property(x => x.EgressWorkspaceId).HasColumnName("egress_workspace_id").HasMaxLength(200);
    builder.Property(x => x.NetappFolderPath).HasColumnName("netapp_folder_path").HasMaxLength(260);
    builder.Property(x => x.ActiveTransferId).HasColumnName("active_transfer_id");
  }
}
