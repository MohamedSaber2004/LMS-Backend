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

            builder.Property(application => application.Specialization)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(application => application.Bio)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(application => application.LinkedInUrl)
                .HasMaxLength(500);

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
            builder.HasIndex(application => application.UserId)
                .IsUnique();
            builder.HasIndex(application => application.IsDeleted);

            builder.HasOne(application => application.User)
                .WithMany()
                .HasForeignKey(application => application.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(application => application.Certificates)
                .WithOne(certificate => certificate.InstructorApplication)
                .HasForeignKey(certificate => certificate.InstructorApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(application => !application.IsDeleted);
        }
    }
}
