using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(course => course.Id);

            builder.Property(course => course.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(course => course.Description)
                .IsRequired();

            builder.Property(course => course.CoverUrl)
                .HasMaxLength(500);

            builder.Property(course => course.Level)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(course => course.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(course => course.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(course => course.Order)
                .IsRequired();

            builder.Property(course => course.CreatedBy)
                .IsRequired();

            builder.Property(course => course.Version)
                .IsRowVersion();

            builder.HasIndex(course => course.Level);
            builder.HasIndex(course => course.Type);
            builder.HasIndex(course => course.Order);
            builder.HasIndex(course => course.IsDeleted);

            builder.HasQueryFilter(course => !course.IsDeleted);
        }
    }
}
