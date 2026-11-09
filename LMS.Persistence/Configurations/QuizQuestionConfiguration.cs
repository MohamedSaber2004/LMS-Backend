using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
    {
        public void Configure(EntityTypeBuilder<QuizQuestion> builder)
        {
            builder.ToTable("QuizQuestions");

            builder.HasKey(question => question.Id);

            builder.Property(question => question.LessonId)
                .IsRequired(false);

            builder.Property(question => question.CourseId)
                .IsRequired(false);

            builder.Property(question => question.Text)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(question => question.Order)
                .IsRequired();

            builder.Property(question => question.CreatedBy)
                .IsRequired();

            builder.Property(question => question.Version)
                .IsRowVersion();

            builder.HasIndex(question => question.LessonId);
            builder.HasIndex(question => question.CourseId);
            builder.HasIndex(question => new { question.LessonId, question.Order });
            builder.HasIndex(question => new { question.CourseId, question.Order });
            builder.HasIndex(question => question.IsDeleted);

            builder.HasOne(question => question.Lesson)
                .WithMany(lesson => lesson.QuizQuestions)
                .HasForeignKey(question => question.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(question => question.Course)
                .WithMany(course => course.FinalQuizQuestions)
                .HasForeignKey(question => question.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(question => !question.IsDeleted);
        }
    }
}
