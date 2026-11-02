using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class InstructorApplicationConfiguration : IEntityTypeConfiguration<InstructorApplication>
    {
        public void Configure(EntityTypeBuilder<InstructorApplication> builder)
        {
            builder.ToTable("InstructorApplications");

            builder.HasKey(application => application.Id);

            builder.Property(application => application.HourlyRate)
                .HasPrecision(10, 2);

            builder.Property(application => application.ReviewNotes)
                .HasMaxLength(1000);

            builder.Property(application => application.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(application => application.CreatedBy)
                .IsRequired();

            builder.Property(application => application.Version)
                .IsRowVersion();

            builder.HasIndex(application => application.Status);
            builder.HasIndex(application => new { application.Status, application.HourlyRate });
            builder.HasIndex(application => application.InstructorId)
                .IsUnique();
            builder.HasIndex(application => application.IsDeleted);

            builder.HasOne(application => application.Instructor)
                .WithOne(instructor => instructor.Application)
                .HasForeignKey<InstructorApplication>(application => application.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(application => !application.IsDeleted);
        }
    }
}
