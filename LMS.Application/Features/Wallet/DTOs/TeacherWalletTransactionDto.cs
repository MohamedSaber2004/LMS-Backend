using LMS.Domain.Enums;

namespace LMS.Application.Features.Wallet.DTOs
{
    public class TeacherWalletTransactionDto
    {
        public Guid Id { get; set; }
        public Guid? SessionId { get; set; }
        public decimal Amount { get; set; } // Net earnings
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public WalletTransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

