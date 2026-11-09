using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LearnerConfiguration : IEntityTypeConfiguration<Learner>
    {
        public void Configure(EntityTypeBuilder<Learner> builder)
        {
            builder.ToTable("Learners");

            builder.HasKey(learner => learner.Id);

            builder.Property(learner => learner.HourlyRate)
                .HasPrecision(10, 2);

            builder.Property(learner => learner.CreatedBy)
                .IsRequired();

            builder.Property(learner => learner.Version)
                .IsRowVersion();

            builder.HasIndex(learner => learner.UserId)
                .IsUnique();
            builder.HasIndex(learner => learner.IsDeleted);

            builder.HasOne(learner => learner.User)
                .WithMany()
                .HasForeignKey(learner => learner.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(learner => !learner.IsDeleted);
        }
    }
}
