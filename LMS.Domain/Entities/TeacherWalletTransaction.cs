using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class TeacherWalletTransaction : BaseEntity<Guid>
    {
        public Guid WalletId { get; private set; }
        public virtual TeacherWallet Wallet { get; private set; } = null!;

        public Guid? SessionId { get; private set; }
        public virtual Session? Session { get; private set; }

        public decimal Amount { get; private set; }
        public decimal GrossAmount { get; private set; }
        public decimal CommissionAmount { get; private set; }
        public decimal CommissionRate { get; private set; }
        public WalletTransactionType Type { get; private set; } = WalletTransactionType.SessionEarning;
        public string Description { get; private set; } = null!;

        public static TeacherWalletTransaction Create(
            Guid walletId,
            Guid? sessionId,
            decimal amount,
            decimal grossAmount,
            decimal commissionAmount,
            decimal commissionRate,
            WalletTransactionType type,
            string description)
        {
            return new TeacherWalletTransaction
            {
                WalletId = walletId,
                SessionId = sessionId,
                Amount = amount,
                GrossAmount = grossAmount,
                CommissionAmount = commissionAmount,
                CommissionRate = commissionRate,
                Type = type,
                Description = description
            };
        }
    }
}

