using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectBranchSelector.DbClient.Context
{
    public class SettingsEntityConfiguration : IEntityTypeConfiguration<Settings>
    {
        public void Configure(EntityTypeBuilder<Settings> builder)
        {
            builder.ToTable("settings");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.ParamName).HasColumnName("param_name");
            builder.Property(s => s.ParamValue).HasColumnName("param_value");
        }
    }
}
