using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class InstructorAvailabilitySlotConfiguration : IEntityTypeConfiguration<InstructorAvailabilitySlot>
    {
        public void Configure(EntityTypeBuilder<InstructorAvailabilitySlot> builder)
        {
            builder.ToTable("InstructorAvailabilitySlots");

            builder.HasKey(slot => slot.Id);

            builder.Property(slot => slot.Day)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(slot => slot.StartTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(slot => slot.EndTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(slot => slot.CreatedBy)
                .IsRequired();

            builder.Property(slot => slot.Version)
                .IsRowVersion();

            builder.HasIndex(slot => slot.InstructorId);
            builder.HasIndex(slot => new { slot.InstructorId, slot.Day, slot.StartTime });
            builder.HasIndex(slot => slot.IsDeleted);

            builder.HasOne(slot => slot.Instructor)
                .WithMany(instructor => instructor.AvailabilitySlots)
                .HasForeignKey(slot => slot.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(slot => !slot.IsDeleted);
        }
    }
}
