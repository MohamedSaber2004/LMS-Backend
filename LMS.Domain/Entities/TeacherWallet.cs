using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class TeacherWallet : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public decimal AvailableBalance { get; private set; }
        public decimal PendingBalance { get; private set; }
        public decimal TotalEarned { get; private set; }

        public virtual ICollection<TeacherWalletTransaction> Transactions { get; private set; } = new List<TeacherWalletTransaction>();

        public static TeacherWallet Create(Guid instructorId)
        {
            return new TeacherWallet
            {
                InstructorId = instructorId,
                AvailableBalance = 0m,
                PendingBalance = 0m,
                TotalEarned = 0m
            };
        }

        public TeacherWalletTransaction CreditSessionEarnings(
            Guid sessionId,
            decimal grossAmount,
            decimal commissionRate,
            string? description = null)
        {
            var commissionAmount = Math.Round(grossAmount * (commissionRate / 100m), 2);
            var netEarnings = grossAmount - commissionAmount;

            AvailableBalance += netEarnings;
            TotalEarned += netEarnings;

            var transaction = TeacherWalletTransaction.Create(
                Id,
                sessionId,
                netEarnings,
                grossAmount,
                commissionAmount,
                commissionRate,
                WalletTransactionType.SessionEarning,
                description ?? $"Earnings for session {sessionId}");

            Transactions.Add(transaction);
            return transaction;
        }

        public TeacherWalletTransaction WithdrawEarnings(decimal amount, string? note = null)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be greater than zero.");
            }

            if (amount > AvailableBalance)
            {
                throw new InvalidOperationException("Withdrawal amount exceeds available balance.");
            }

            AvailableBalance -= amount;

            var transaction = TeacherWalletTransaction.Create(
                Id,
                null,
                -amount,
                amount,
                0,
                0,
                WalletTransactionType.PayoutWithdrawal,
                note ?? $"Payout withdrawal request of {amount:N2}");

            Transactions.Add(transaction);
            return transaction;
        }
    }
}

