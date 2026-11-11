using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class PackageTierConfiguration : IEntityTypeConfiguration<PackageTier>
    {
        public void Configure(EntityTypeBuilder<PackageTier> builder)
        {
            builder.ToTable("PackageTiers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(t => t.TotalHours)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.Property(t => t.TotalFullSessions)
                .IsRequired();

            builder.Property(t => t.TotalHalfSessions)
                .IsRequired();

            builder.Property(t => t.PricePerHour)
                .HasPrecision(10, 2)
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(t => t.TotalPrice)
                .HasPrecision(10, 2)
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.DisplayOrder)
                .IsRequired();

            builder.Property(t => t.CreatedBy)
                .IsRequired();

            builder.Property(t => t.Version)
                .IsRowVersion();

            builder.HasIndex(t => t.DisplayOrder);
            builder.HasIndex(t => t.TotalHours);
            builder.HasIndex(t => t.IsDeleted);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
