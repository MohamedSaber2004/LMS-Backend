using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class LiveRoomConfiguration : IEntityTypeConfiguration<LiveRoom>
    {
        public void Configure(EntityTypeBuilder<LiveRoom> builder)
        {
            builder.ToTable("LiveRooms");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ChannelName)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(r => r.Version)
                .IsRowVersion();

            builder.HasIndex(r => r.SessionId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(r => r.ChannelName);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.HostUserId);
            builder.HasIndex(r => r.IsDeleted);

            builder.HasOne(r => r.Session)
                .WithOne(s => s.LiveRoom)
                .HasForeignKey<LiveRoom>(r => r.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.HostUser)
                .WithMany()
                .HasForeignKey(r => r.HostUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}

