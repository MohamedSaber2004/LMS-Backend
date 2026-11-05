using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class CourseInstructorConfiguration : IEntityTypeConfiguration<CourseInstructor>
    {
        public void Configure(EntityTypeBuilder<CourseInstructor> builder)
        {
            builder.ToTable("CourseInstructors");

            builder.HasKey(courseInstructor => courseInstructor.Id);

            builder.Property(courseInstructor => courseInstructor.CreatedBy)
                .IsRequired();

            builder.Property(courseInstructor => courseInstructor.Version)
                .IsRowVersion();

            builder.HasIndex(courseInstructor => new { courseInstructor.CourseId, courseInstructor.InstructorId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
            builder.HasIndex(courseInstructor => courseInstructor.InstructorId);
            builder.HasIndex(courseInstructor => courseInstructor.IsDeleted);

            builder.HasOne(courseInstructor => courseInstructor.Course)
                .WithMany(course => course.CourseInstructors)
                .HasForeignKey(courseInstructor => courseInstructor.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(courseInstructor => courseInstructor.Instructor)
                .WithMany(instructor => instructor.CourseInstructors)
                .HasForeignKey(courseInstructor => courseInstructor.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(courseInstructor => !courseInstructor.IsDeleted);
        }
    }
}
