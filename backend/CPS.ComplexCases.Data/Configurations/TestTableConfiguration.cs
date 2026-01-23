using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Constants;

namespace CPS.ComplexCases.Data.Configurations;

public class TestTableConfiguration : IEntityTypeConfiguration<TestTable>
{
  public void Configure(EntityTypeBuilder<TestTable> builder)
  {
    builder.ToTable("test_table", SchemaNames.Lcc);
    builder.HasKey(x => x.TestId);

    builder.Property(x => x.TestId).HasColumnName("test_id").IsRequired();
    builder.Property(x => x.TestColumn).HasColumnName("test_column").HasMaxLength(200);
  }
}
