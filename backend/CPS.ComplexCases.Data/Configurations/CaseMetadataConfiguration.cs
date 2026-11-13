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
        builder.Property(x => x.EgressWorkspaceName).HasColumnName("egress_workspace_name").HasMaxLength(200);
        builder.Property(x => x.EgressWorkspaceId).HasColumnName("egress_workspace_id").HasMaxLength(200);
        builder.Property(x => x.NetappFolderPath).HasColumnName("netapp_folder_path").HasMaxLength(260);
        builder.Property(x => x.NetappBucketName).HasColumnName("netapp_bucket_name").HasMaxLength(200);
        builder.Property(x => x.ActiveTransferId).HasColumnName("active_transfer_id");

        builder.HasIndex(x => x.EgressWorkspaceId)
            .IsUnique()
            .HasFilter("egress_workspace_id IS NOT NULL")
            .HasDatabaseName("ux_case_metadata_egress_workspace_id");
        builder.HasIndex(x => x.NetappFolderPath)
            .IsUnique()
            .HasFilter("netapp_folder_path IS NOT NULL")
            .HasDatabaseName("ux_case_metadata_netapp_folder_path");
    }
}
