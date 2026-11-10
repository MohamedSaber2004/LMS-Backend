using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class SessionReviewConfiguration : IEntityTypeConfiguration<SessionReview>
    {
        public void Configure(EntityTypeBuilder<SessionReview> builder)
        {
            builder.ToTable("SessionReviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedBy)
                .IsRequired();

            builder.Property(r => r.Version)
                .IsRowVersion();

            builder.HasIndex(r => r.SessionId)
                .IsUnique();
            builder.HasIndex(r => r.InstructorId);
            builder.HasIndex(r => r.LearnerId);
            builder.HasIndex(r => r.IsDeleted);

            builder.HasOne(r => r.Session)
                .WithMany()
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Learner)
                .WithMany()
                .HasForeignKey(r => r.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Instructor)
                .WithMany(i => i.Reviews)
                .HasForeignKey(r => r.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}

