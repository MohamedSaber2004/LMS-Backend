using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CountryEnName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(c => c.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(c => c.Code);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(c => c.CreatedBy)
                .IsRequired(false);

            builder.Property(c => c.UpdatedBy)
                .IsRequired(false);

            builder.Property(c => c.DeletedBy)
                .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            builder.Property(c => c.DeletedAt)
                .IsRequired(false);

            builder.Property(c => c.Version)
                .IsRowVersion();

            builder.HasIndex(c => c.IsDeleted);

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
