using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.ToTable("QuizAttempts");

            builder.HasKey(attempt => attempt.Id);

            builder.Property(attempt => attempt.Score)
                .IsRequired();

            builder.Property(attempt => attempt.TotalQuestions)
                .IsRequired();

            builder.Property(attempt => attempt.CreatedBy)
                .IsRequired();

            builder.Property(attempt => attempt.Version)
                .IsRowVersion();

            builder.HasIndex(attempt => new { attempt.LessonId, attempt.LearnerId });
            builder.HasIndex(attempt => attempt.LearnerId);
            builder.HasIndex(attempt => attempt.IsDeleted);

            builder.HasOne(attempt => attempt.Lesson)
                .WithMany()
                .HasForeignKey(attempt => attempt.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(attempt => attempt.Learner)
                .WithMany()
                .HasForeignKey(attempt => attempt.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(attempt => !attempt.IsDeleted);
        }
    }
}
