using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectBranchSelector.DbClient.Context
{
    public class FormulaEntityConfiguration : IEntityTypeConfiguration<Formula>
    {
        public void Configure(EntityTypeBuilder<Formula> builder)
        {
            builder.ToTable("formula");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).HasColumnName("name");
            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.IsDeleted).HasColumnName("is_deleted");
            builder.Property(s => s.VersionDate).HasColumnName("version_date");
            builder.Property(s => s.IsDefault).HasColumnName("is_default");
            builder.Property(s => s.Text).HasColumnName("text");
            builder.Property(s => s.IsSync).HasColumnName("is_sync");
        }
    }
}
