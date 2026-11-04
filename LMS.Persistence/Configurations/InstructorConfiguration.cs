using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.ToTable("Instructors");

            builder.HasKey(instructor => instructor.Id);

            builder.Property(instructor => instructor.Specialization)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(instructor => instructor.Bio)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(instructor => instructor.Education)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(instructor => instructor.LinkedInUrl)
                .HasMaxLength(500);

            builder.Property(instructor => instructor.HourlyRate)
                .HasPrecision(10, 2);

            builder.Property(instructor => instructor.CreatedBy)
                .IsRequired();

            builder.Property(instructor => instructor.Version)
                .IsRowVersion();

            builder.HasIndex(instructor => instructor.UserId)
                .IsUnique();
            builder.HasIndex(instructor => instructor.IsDeleted);

            builder.HasOne(instructor => instructor.User)
                .WithMany()
                .HasForeignKey(instructor => instructor.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(instructor => instructor.Certificates)
                .WithOne(certificate => certificate.Instructor)
                .HasForeignKey(certificate => certificate.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(instructor => !instructor.IsDeleted);
        }
    }
}
