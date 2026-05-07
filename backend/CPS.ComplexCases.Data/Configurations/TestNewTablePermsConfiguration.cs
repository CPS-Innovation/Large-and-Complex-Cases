using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Constants;

namespace CPS.ComplexCases.Data.Configurations;

public class TestNewTablePermsConfiguration : IEntityTypeConfiguration<TestNewTablePerms>
{
  public void Configure(EntityTypeBuilder<TestNewTablePerms> builder)
  {
    builder.ToTable("test_new_table_perms", SchemaNames.Lcc);
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).HasColumnName("id").IsRequired();
    builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(200);
  }
}
