using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public class OnboardingOptionConfiguration : IEntityTypeConfiguration<OnboardingOption>
    {
        public void Configure(EntityTypeBuilder<OnboardingOption> builder)
        {
            builder.ToTable("OnboardingOptions");

            builder.HasKey(option => option.Id);

            builder.Property(option => option.Label)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(option => option.Value)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(option => option.CreatedBy)
                .IsRequired(false);

            builder.Property(option => option.Version)
                .IsRowVersion();

            builder.HasIndex(option => option.IsDeleted);

            builder.HasQueryFilter(option => !option.IsDeleted);
        }
    }
}
