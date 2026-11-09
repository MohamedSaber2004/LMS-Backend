using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.ToTable("Lessons");

            builder.HasKey(lesson => lesson.Id);

            builder.Property(lesson => lesson.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(lesson => lesson.VideoUrl)
                .HasMaxLength(500);

            builder.Property(lesson => lesson.StudyBookUrl)
                .HasMaxLength(500);

            builder.Property(lesson => lesson.Order)
                .IsRequired();

            builder.Property(lesson => lesson.IsFreeDemo)
                .IsRequired();

            builder.Property(lesson => lesson.CreatedBy)
                .IsRequired();

            builder.Property(lesson => lesson.Version)
                .IsRowVersion();

            builder.HasIndex(lesson => lesson.CourseId);
            builder.HasIndex(lesson => new { lesson.CourseId, lesson.Order });
            builder.HasIndex(lesson => lesson.IsDeleted);

            builder.HasOne(lesson => lesson.Course)
                .WithMany(course => course.Lessons)
                .HasForeignKey(lesson => lesson.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(lesson => !lesson.IsDeleted);
        }
    }
}
