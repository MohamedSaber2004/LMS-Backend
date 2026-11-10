namespace LMS.Application.Features.Wallet.DTOs
{
    public class TeacherWalletSummaryDto
    {
        public Guid WalletId { get; set; }
        public Guid InstructorId { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal CommissionRate { get; set; } // CCR %
    }
}

