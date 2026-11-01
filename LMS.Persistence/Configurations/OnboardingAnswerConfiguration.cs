using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public class OnboardingAnswerConfiguration : IEntityTypeConfiguration<OnboardingAnswer>
    {
        public void Configure(EntityTypeBuilder<OnboardingAnswer> builder)
        {
            builder.ToTable("OnboardingAnswers");

            builder.HasKey(answer => answer.Id);

            builder.Property(answer => answer.NumericValue).HasPrecision(10, 2);

            builder.Property(answer => answer.CreatedBy)
                .IsRequired(false);

            builder.Property(answer => answer.Version)
                .IsRowVersion();

            builder.HasIndex(answer => answer.IsDeleted);
            builder.HasIndex(answer => new { answer.UserId, answer.OnboardingQuestionId });

            builder.HasOne(answer => answer.User)
                .WithMany()
                .HasForeignKey(answer => answer.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(answer => answer.OnboardingQuestion)
                .WithMany()
                .HasForeignKey(answer => answer.OnboardingQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(answer => answer.OnboardingOption)
                .WithMany()
                .HasForeignKey(answer => answer.OnboardingOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(answer => !answer.IsDeleted);
        }
    }
}
