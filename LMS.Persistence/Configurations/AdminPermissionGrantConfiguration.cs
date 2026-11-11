using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class AdminPermissionGrantConfiguration : IEntityTypeConfiguration<AdminPermissionGrant>
    {
        public void Configure(EntityTypeBuilder<AdminPermissionGrant> builder)
        {
            builder.ToTable("AdminPermissionGrants");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.UserId)
                .IsRequired();

            builder.Property(g => g.Permission)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(g => new { g.UserId, g.Permission })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(g => g.Permission);
            builder.HasIndex(g => g.IsDeleted);

            builder.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
