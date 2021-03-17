using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectBranchSelector.DbClient.Context
{
    public class TreeEntityConfiguration : IEntityTypeConfiguration<Tree>
    {
        public void Configure(EntityTypeBuilder<Tree> builder)
        {
            builder.ToTable("tree");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Description).HasColumnName("description");
            builder.Property(s => s.FormulaId).HasColumnName("formula_id");
            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.IsDeleted).HasColumnName("is_deleted");
            builder.Property(s => s.Name).HasColumnName("name");
            builder.Property(s => s.VersionDate).HasColumnName("version_date");
            builder.Property(s => s.IsSync).HasColumnName("is_sync");
            builder.Ignore(s => s.Formula);
        }
    }
}
