using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(session => session.Id);

            builder.Property(session => session.Title)
                .HasMaxLength(200);

            builder.Property(session => session.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(session => session.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(session => session.StartsAtUtc)
                .IsRequired();

            builder.Property(session => session.EndsAtUtc)
                .IsRequired();

            builder.Property(session => session.Price)
                .HasPrecision(10, 2);

            builder.Property(session => session.DurationMinutes)
                .IsRequired()
                .HasDefaultValue(50);

            builder.Property(session => session.IsTrial)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(session => session.IsPaid)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(session => session.PaidAtUtc);

            builder.Property(session => session.HourlyRateSnapshot)
                .HasPrecision(10, 2)
                .HasDefaultValue(0m);

            builder.Property(session => session.CommissionRateSnapshot)
                .HasPrecision(5, 2)
                .HasDefaultValue(0m);

            builder.Property(session => session.IsAttendanceConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(session => session.AttendanceConfirmedAtUtc);

            builder.Property(session => session.CancelReason)
                .HasMaxLength(1000);

            builder.Property(session => session.CreatedBy)
                .IsRequired();

            builder.Property(session => session.Version)
                .IsRowVersion();

            builder.HasIndex(session => session.InstructorId);
            builder.HasIndex(session => session.LessonId)
                .IsUnique()
                .HasFilter("[LessonId] IS NOT NULL");
            builder.HasIndex(session => new { session.InstructorId, session.StartsAtUtc });
            builder.HasIndex(session => new { session.StartsAtUtc, session.EndsAtUtc });
            builder.HasIndex(session => session.Status);
            builder.HasIndex(session => session.IsDeleted);

            builder.HasOne(session => session.Instructor)
                .WithMany(instructor => instructor.Sessions)
                .HasForeignKey(session => session.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(session => session.Lesson)
                .WithOne(lesson => lesson.Session)
                .HasForeignKey<Session>(session => session.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(session => !session.IsDeleted);
        }
    }
}
