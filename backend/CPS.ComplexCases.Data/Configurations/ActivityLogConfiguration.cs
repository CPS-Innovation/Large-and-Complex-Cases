using CPS.ComplexCases.Data.Constants;
using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace CPS.ComplexCases.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_log", SchemaNames.Lcc);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
        builder.Property(x => x.CaseId).HasColumnName("case_id");
        builder.Property(x => x.ActionType).HasColumnName("action_type").HasMaxLength(50);
        builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(100);
        builder.Property(x => x.ResourceType).HasColumnName("resource_type").HasMaxLength(50);
        builder.Property(x => x.ResourceId).HasColumnName("resource_id").HasMaxLength(50);
        builder.Property(x => x.ResourceName).HasColumnName("resource_name").HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.Property(x => x.Details).HasColumnName("details").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.ActionType).HasDatabaseName("idx_activity_log_action_type");
        builder.HasIndex(x => x.UserName).HasDatabaseName("idx_activity_log_user_name");
        builder.HasIndex(x => x.ResourceId).HasDatabaseName("idx_activity_log_resource_id");
        builder.HasIndex(x => x.Timestamp).HasDatabaseName("idx_activity_log_timestamp").IsDescending();
    }
}