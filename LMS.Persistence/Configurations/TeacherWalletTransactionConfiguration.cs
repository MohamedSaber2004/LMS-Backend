using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class TeacherWalletTransactionConfiguration : IEntityTypeConfiguration<TeacherWalletTransaction>
    {
        public void Configure(EntityTypeBuilder<TeacherWalletTransaction> builder)
        {
            builder.ToTable("TeacherWalletTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(t => t.GrossAmount)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(t => t.CommissionAmount)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(t => t.CommissionRate)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(t => t.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.CreatedBy)
                .IsRequired();

            builder.Property(t => t.Version)
                .IsRowVersion();

            builder.HasIndex(t => t.WalletId);
            builder.HasIndex(t => t.SessionId);
            builder.HasIndex(t => t.IsDeleted);

            builder.HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Session)
                .WithMany()
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
