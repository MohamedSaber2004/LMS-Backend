using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class QuizAttemptAnswerConfiguration : IEntityTypeConfiguration<QuizAttemptAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAttemptAnswer> builder)
        {
            builder.ToTable("QuizAttemptAnswers");

            builder.HasKey(answer => answer.Id);

            builder.Property(answer => answer.QuestionId)
                .IsRequired();

            builder.Property(answer => answer.SelectedOptionId)
                .IsRequired();

            builder.Property(answer => answer.CreatedBy)
                .IsRequired();

            builder.Property(answer => answer.Version)
                .IsRowVersion();

            builder.HasIndex(answer => answer.QuizAttemptId);
            builder.HasIndex(answer => answer.IsDeleted);

            builder.HasOne(answer => answer.QuizAttempt)
                .WithMany(attempt => attempt.Answers)
                .HasForeignKey(answer => answer.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(answer => !answer.IsDeleted);
        }
    }
}
