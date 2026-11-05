using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
    {
        public void Configure(EntityTypeBuilder<QuizOption> builder)
        {
            builder.ToTable("QuizOptions");

            builder.HasKey(option => option.Id);

            builder.Property(option => option.Text)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(option => option.IsCorrect)
                .IsRequired();

            builder.Property(option => option.Order)
                .IsRequired();

            builder.Property(option => option.CreatedBy)
                .IsRequired();

            builder.Property(option => option.Version)
                .IsRowVersion();

            builder.HasIndex(option => option.QuizQuestionId);
            builder.HasIndex(option => new { option.QuizQuestionId, option.Order });
            builder.HasIndex(option => option.IsDeleted);

            builder.HasOne(option => option.QuizQuestion)
                .WithMany(question => question.Options)
                .HasForeignKey(option => option.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(option => !option.IsDeleted);
        }
    }
}
