using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LiveRoomParticipantConfiguration : IEntityTypeConfiguration<LiveRoomParticipant>
    {
        public void Configure(EntityTypeBuilder<LiveRoomParticipant> builder)
        {
            builder.ToTable("LiveRoomParticipants");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.AgoraUid)
                .IsRequired();

            builder.Property(p => p.JoinedAtUtc)
                .IsRequired();

            builder.Property(p => p.Version)
                .IsRowVersion();

            builder.HasIndex(p => p.RoomId);
            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => new { p.RoomId, p.UserId });
            builder.HasIndex(p => p.IsDeleted);

            builder.HasOne(p => p.Room)
                .WithMany(r => r.Participants)
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}

