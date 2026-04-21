using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Constants;

namespace CPS.ComplexCases.Data.Configurations;

public class CaseActiveManageMaterialsConfiguration : IEntityTypeConfiguration<CaseActiveManageMaterialsOperation>
{
    public void Configure(EntityTypeBuilder<CaseActiveManageMaterialsOperation> builder)
    {
        builder.ToTable("case_active_manage_materials", SchemaNames.Lcc);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").IsRequired();
        builder.Property(x => x.CaseId).HasColumnName("case_id").IsRequired();
        builder.Property(x => x.OperationType).HasColumnName("operation_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SourcePaths).HasColumnName("source_paths").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.DestinationPaths).HasColumnName("destination_paths").HasColumnType("jsonb");
        builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(200);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.CaseId).HasDatabaseName("idx_case_active_manage_materials_case_id");
    }
}
