using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("UserRefreshTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                .IsRequired();

            builder.HasIndex(t => t.Token)
                .IsUnique();

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.HasIndex(t => t.UserId);

            builder.Property(t => t.ExpiryDate)
                .IsRequired();

            builder.Property(t => t.IsRevoked)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(t => t.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.CreatedBy)
                .IsRequired(false);

            builder.Property(t => t.UpdatedBy)
                .IsRequired(false);

            builder.Property(t => t.DeletedBy)
                .IsRequired(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .IsRequired(false);

            builder.Property(t => t.DeletedAt)
                .IsRequired(false);

            builder.Property(t => t.Version)
                .IsRowVersion();

            builder.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.IsDeleted);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
