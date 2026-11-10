using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class TeacherWalletConfiguration : IEntityTypeConfiguration<TeacherWallet>
    {
        public void Configure(EntityTypeBuilder<TeacherWallet> builder)
        {
            builder.ToTable("TeacherWallets");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.AvailableBalance)
                .HasPrecision(12, 2)
                .HasDefaultValue(0m);

            builder.Property(w => w.PendingBalance)
                .HasPrecision(12, 2)
                .HasDefaultValue(0m);

            builder.Property(w => w.TotalEarned)
                .HasPrecision(12, 2)
                .HasDefaultValue(0m);

            builder.Property(w => w.CreatedBy)
                .IsRequired();

            builder.Property(w => w.Version)
                .IsRowVersion();

            builder.HasIndex(w => w.InstructorId)
                .IsUnique();
            builder.HasIndex(w => w.IsDeleted);

            builder.HasOne(w => w.Instructor)
                .WithOne(i => i.Wallet)
                .HasForeignKey<TeacherWallet>(w => w.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(w => !w.IsDeleted);
        }
    }
}

