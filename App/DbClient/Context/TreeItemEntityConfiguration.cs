using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectBranchSelector.DbClient.Context
{
    public class TreeItemEntityConfiguration : IEntityTypeConfiguration<TreeItem>
    {
        public void Configure(EntityTypeBuilder<TreeItem> builder)
        {
            builder.ToTable("tree_item");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.AddFields).HasColumnName("add_fields");
            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.IsDeleted).HasColumnName("is_deleted");
            builder.Property(s => s.Name).HasColumnName("name");
            builder.Property(s => s.Description).HasColumnName("description");
            builder.Property(s => s.VersionDate).HasColumnName("version_date");
            builder.Property(s => s.IsLeaf).HasColumnName("is_leaf");
            builder.Property(s => s.ParentId).HasColumnName("parent_id");
            builder.Property(s => s.SelectCount).HasColumnName("select_count");
            builder.Property(s => s.TreeId).HasColumnName("tree_id");
            builder.Property(s => s.Weight).HasColumnName("weight");
            builder.Property(s => s.IsSync).HasColumnName("is_sync");
        }
    }
}
