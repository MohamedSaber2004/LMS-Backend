using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class AvailabilityInstanceConfiguration : IEntityTypeConfiguration<AvailabilityInstance>
    {
        public void Configure(EntityTypeBuilder<AvailabilityInstance> builder)
        {
            builder.ToTable("AvailabilityInstances");

            builder.HasKey(instance => instance.Id);

            builder.Property(instance => instance.Date)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(instance => instance.StartsAtUtc)
                .IsRequired();

            builder.Property(instance => instance.EndsAtUtc)
                .IsRequired();

            builder.Property(instance => instance.IsOpen)
                .IsRequired();

            builder.Property(instance => instance.CreatedBy)
                .IsRequired();

            builder.Property(instance => instance.Version)
                .IsRowVersion();

            builder.HasIndex(instance => instance.InstructorId);
            builder.HasIndex(instance => new { instance.InstructorId, instance.Date });
            builder.HasIndex(instance => new { instance.StartsAtUtc, instance.EndsAtUtc });
            builder.HasIndex(instance => instance.IsDeleted);

            builder.HasOne(instance => instance.Instructor)
                .WithMany(instructor => instructor.AvailabilityInstances)
                .HasForeignKey(instance => instance.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(instance => !instance.IsDeleted);
        }
    }
}
