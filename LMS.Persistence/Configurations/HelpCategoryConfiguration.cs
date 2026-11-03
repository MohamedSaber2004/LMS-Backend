using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class HelpCategoryConfiguration : IEntityTypeConfiguration<HelpCategory>
    {
        public void Configure(EntityTypeBuilder<HelpCategory> builder)
        {
            builder.ToTable("HelpCategories");

            builder.HasKey(category => category.Id);

            builder.Property(category => category.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(category => category.Order)
                .IsRequired();

            builder.Property(category => category.CreatedBy)
                .IsRequired();

            builder.Property(category => category.Version)
                .IsRowVersion();

            // Filtered so a soft-deleted category does not keep its name reserved forever.
            builder.HasIndex(category => category.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(category => category.Order);
            builder.HasIndex(category => category.IsDeleted);

            builder.HasQueryFilter(category => !category.IsDeleted);
        }
    }
}
