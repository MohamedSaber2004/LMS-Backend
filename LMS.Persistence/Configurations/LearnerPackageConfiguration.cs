using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LearnerPackageConfiguration : IEntityTypeConfiguration<LearnerPackage>
    {
        public void Configure(EntityTypeBuilder<LearnerPackage> builder)
        {
            builder.ToTable("LearnerPackages");

            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.PackageName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(lp => lp.TotalHours)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.Property(lp => lp.RemainingHours)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.Property(lp => lp.HourlyRateSnapshot)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(lp => lp.TotalPrice)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(lp => lp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(lp => lp.CreatedBy)
                .IsRequired();

            builder.Property(lp => lp.Version)
                .IsRowVersion();

            builder.HasIndex(lp => lp.LearnerId);
            builder.HasIndex(lp => lp.InstructorId);
            builder.HasIndex(lp => new { lp.LearnerId, lp.InstructorId, lp.IsActive });
            builder.HasIndex(lp => lp.IsDeleted);

            builder.HasOne(lp => lp.Learner)
                .WithMany(l => l.Packages)
                .HasForeignKey(lp => lp.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(lp => lp.Instructor)
                .WithMany(i => i.LearnerPackages)
                .HasForeignKey(lp => lp.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(lp => !lp.IsDeleted);
        }
    }
}
