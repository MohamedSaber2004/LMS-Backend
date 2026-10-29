using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class InstructorCertificateConfiguration : IEntityTypeConfiguration<InstructorCertificate>
    {
        public void Configure(EntityTypeBuilder<InstructorCertificate> builder)
        {
            builder.ToTable("InstructorCertificates");

            builder.HasKey(certificate => certificate.Id);

            builder.Property(certificate => certificate.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(certificate => certificate.OriginalFileName)
                .HasMaxLength(260)
                .IsRequired();

            builder.Property(certificate => certificate.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(certificate => certificate.CreatedBy)
                .IsRequired();

            builder.Property(certificate => certificate.Version)
                .IsRowVersion();

            builder.HasIndex(certificate => certificate.InstructorApplicationId);
            builder.HasIndex(certificate => certificate.IsDeleted);

            builder.HasQueryFilter(certificate => !certificate.IsDeleted);
        }
    }
}
