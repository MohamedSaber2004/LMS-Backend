using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class SessionBookingConfiguration : IEntityTypeConfiguration<SessionBooking>
    {
        public void Configure(EntityTypeBuilder<SessionBooking> builder)
        {
            builder.ToTable("SessionBookings");

            builder.HasKey(booking => booking.Id);

            builder.Property(booking => booking.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(booking => booking.CancelReason)
                .HasMaxLength(1000);

            builder.Property(booking => booking.CreatedBy)
                .IsRequired();

            builder.Property(booking => booking.Version)
                .IsRowVersion();

            builder.HasIndex(booking => new { booking.SessionId, booking.LearnerId })
                .IsUnique();
            builder.HasIndex(booking => booking.LearnerId);
            builder.HasIndex(booking => booking.Status);
            builder.HasIndex(booking => booking.IsDeleted);

            builder.HasOne(booking => booking.Session)
                .WithMany(session => session.Bookings)
                .HasForeignKey(booking => booking.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(booking => booking.Learner)
                .WithMany()
                .HasForeignKey(booking => booking.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(booking => !booking.IsDeleted);
        }
    }
}
