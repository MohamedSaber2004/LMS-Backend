using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> builder)
        {
            builder.ToTable("LessonProgress");

            builder.HasKey(progress => progress.Id);

            builder.Property(progress => progress.LastWatchedSeconds)
                .IsRequired();

            builder.Property(progress => progress.IsCompleted)
                .IsRequired();

            builder.Property(progress => progress.CreatedBy)
                .IsRequired();

            builder.Property(progress => progress.Version)
                .IsRowVersion();

            builder.HasIndex(progress => new { progress.LessonId, progress.LearnerId })
                .IsUnique();
            builder.HasIndex(progress => progress.LearnerId);
            builder.HasIndex(progress => progress.IsDeleted);

            builder.HasOne(progress => progress.Lesson)
                .WithMany()
                .HasForeignKey(progress => progress.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(progress => progress.Learner)
                .WithMany()
                .HasForeignKey(progress => progress.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(progress => !progress.IsDeleted);
        }
    }
}
