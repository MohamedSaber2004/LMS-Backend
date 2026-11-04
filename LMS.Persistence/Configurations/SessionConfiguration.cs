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

            builder.Property(session => session.CancelReason)
                .HasMaxLength(1000);

            builder.Property(session => session.CreatedBy)
                .IsRequired();

            builder.Property(session => session.Version)
                .IsRowVersion();

            builder.HasIndex(session => session.InstructorId);
            builder.HasIndex(session => new { session.InstructorId, session.StartsAtUtc });
            builder.HasIndex(session => new { session.StartsAtUtc, session.EndsAtUtc });
            builder.HasIndex(session => session.Status);
            builder.HasIndex(session => session.IsDeleted);

            builder.HasOne(session => session.Instructor)
                .WithMany(instructor => instructor.Sessions)
                .HasForeignKey(session => session.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(session => !session.IsDeleted);
        }
    }
}
