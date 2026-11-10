namespace LMS.Application.Features.Wallet.DTOs
{
    public class AdminFinancialOverviewDto
    {
        public decimal TotalGrossRevenue { get; set; }
        public decimal TotalPlatformCommissionEarned { get; set; }
        public decimal TotalTeacherWalletsBalance { get; set; }
        public decimal TotalTeacherEarningsPaid { get; set; }
        public decimal TotalPackageHoursLiability { get; set; }
        public int TotalCompletedSessions { get; set; }
        public int TotalActiveTeachers { get; set; }
        public int TotalActiveLearners { get; set; }
    }
}

