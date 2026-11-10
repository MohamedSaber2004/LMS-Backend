using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class PackageHourAdjustmentConfiguration : IEntityTypeConfiguration<PackageHourAdjustment>
    {
        public void Configure(EntityTypeBuilder<PackageHourAdjustment> builder)
        {
            builder.ToTable("PackageHourAdjustments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.OldRemainingHours)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.Property(a => a.NewRemainingHours)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.Property(a => a.Reason)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.CreatedBy)
                .IsRequired();

            builder.Property(a => a.Version)
                .IsRowVersion();

            builder.HasIndex(a => a.PackageId);
            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => a.IsDeleted);

            builder.HasOne(a => a.Package)
                .WithMany()
                .HasForeignKey(a => a.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
