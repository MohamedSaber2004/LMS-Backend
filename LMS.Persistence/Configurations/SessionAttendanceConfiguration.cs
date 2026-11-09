using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class SessionAttendanceConfiguration : IEntityTypeConfiguration<SessionAttendance>
    {
        public void Configure(EntityTypeBuilder<SessionAttendance> builder)
        {
            builder.ToTable("SessionAttendances");

            builder.HasKey(attendance => attendance.Id);

            builder.Property(attendance => attendance.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(attendance => attendance.Notes)
                .HasMaxLength(500);

            builder.Property(attendance => attendance.CreatedBy)
                .IsRequired();

            builder.Property(attendance => attendance.Version)
                .IsRowVersion();

            builder.HasIndex(attendance => new { attendance.SessionId, attendance.LearnerId })
                .IsUnique();
            builder.HasIndex(attendance => attendance.LearnerId);
            builder.HasIndex(attendance => attendance.Status);
            builder.HasIndex(attendance => attendance.IsDeleted);

            builder.HasOne(attendance => attendance.Session)
                .WithMany(session => session.Attendances)
                .HasForeignKey(attendance => attendance.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(attendance => attendance.Learner)
                .WithMany(learner => learner.Attendances)
                .HasForeignKey(attendance => attendance.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(attendance => !attendance.IsDeleted);
        }
    }
}

