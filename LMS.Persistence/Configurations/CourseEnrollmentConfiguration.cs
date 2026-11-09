using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
    {
        public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
        {
            builder.ToTable("CourseEnrollments");

            builder.HasKey(enrollment => enrollment.Id);

            builder.Property(enrollment => enrollment.CreatedBy)
                .IsRequired();

            builder.Property(enrollment => enrollment.Version)
                .IsRowVersion();

            builder.HasIndex(enrollment => new { enrollment.CourseId, enrollment.LearnerId })
                .IsUnique();
            builder.HasIndex(enrollment => enrollment.LearnerId);
            builder.HasIndex(enrollment => enrollment.IsDeleted);

            builder.HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(enrollment => enrollment.Learner)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(enrollment => !enrollment.IsDeleted);
        }
    }
}
