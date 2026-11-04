using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class RescheduleRequestConfiguration : IEntityTypeConfiguration<RescheduleRequest>
    {
        public void Configure(EntityTypeBuilder<RescheduleRequest> builder)
        {
            builder.ToTable("RescheduleRequests");

            builder.HasKey(request => request.Id);

            builder.Property(request => request.RequestedByType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(request => request.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(request => request.FromUtc)
                .IsRequired();

            builder.Property(request => request.ToUtc)
                .IsRequired();

            builder.Property(request => request.CreatedBy)
                .IsRequired();

            builder.Property(request => request.Version)
                .IsRowVersion();

            builder.HasIndex(request => request.SessionId);
            builder.HasIndex(request => new { request.SessionId, request.Status });
            builder.HasIndex(request => request.RequestedByUserId);
            builder.HasIndex(request => request.IsDeleted);

            builder.HasOne(request => request.Session)
                .WithMany(session => session.RescheduleRequests)
                .HasForeignKey(request => request.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(request => !request.IsDeleted);
        }
    }
}
