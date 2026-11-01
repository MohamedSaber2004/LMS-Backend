using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public class OnboardingQuestionConfiguration : IEntityTypeConfiguration<OnboardingQuestion>
    {
        public void Configure(EntityTypeBuilder<OnboardingQuestion> builder)
        {
            builder.ToTable("OnboardingQuestions");

            builder.HasKey(question => question.Id);

            builder.Property(question => question.Key)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(question => question.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(question => question.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(question => question.MinValue).HasPrecision(10, 2);
            builder.Property(question => question.MaxValue).HasPrecision(10, 2);
            builder.Property(question => question.Step).HasPrecision(10, 2);

            builder.Property(question => question.CreatedBy)
                .IsRequired(false);

            builder.Property(question => question.Version)
                .IsRowVersion();

            builder.HasIndex(question => question.Key)
                .IsUnique();

            builder.HasIndex(question => question.IsDeleted);

            builder.HasMany(question => question.Options)
                .WithOne(option => option.OnboardingQuestion)
                .HasForeignKey(option => option.OnboardingQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(question => !question.IsDeleted);
        }
    }
}
