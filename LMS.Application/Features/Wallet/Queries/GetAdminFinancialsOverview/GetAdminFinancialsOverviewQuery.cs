using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Wallet.Queries.GetAdminFinancialsOverview
{
    public class GetAdminFinancialsOverviewQuery : IRequest<Result<AdminFinancialOverviewDto>>
    {
    }

    public class GetAdminFinancialsOverviewQueryHandler : IRequestHandler<GetAdminFinancialsOverviewQuery, Result<AdminFinancialOverviewDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminFinancialsOverviewQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<AdminFinancialOverviewDto>> Handle(
            GetAdminFinancialsOverviewQuery request,
            CancellationToken cancellationToken)
        {
            var grossRevenue = await _context.LearnerPackages
                .SumAsync(lp => (decimal?)lp.TotalPrice, cancellationToken) ?? 0m;

            var platformCommission = await _context.TeacherWalletTransactions
                .Where(t => t.Type == WalletTransactionType.SessionEarning
                    && t.Status != WalletTransactionStatus.Reversed)
                .SumAsync(t => (decimal?)t.CommissionAmount, cancellationToken) ?? 0m;

            var walletsAvailable = await _context.TeacherWallets
                .SumAsync(w => (decimal?)w.AvailableBalance, cancellationToken) ?? 0m;

            var walletsTotalEarned = await _context.TeacherWallets
                .SumAsync(w => (decimal?)w.TotalEarned, cancellationToken) ?? 0m;

            var packageHoursLiability = await _context.LearnerPackages
                .Where(lp => lp.IsActive)
                .SumAsync(lp => (decimal?)lp.RemainingHours, cancellationToken) ?? 0m;

            var completedSessions = await _context.Sessions
                .CountAsync(s => s.Status == SessionStatus.Completed, cancellationToken);

            var activeTeachers = await _context.Instructors
                .CountAsync(i => i.User.IsActive && !i.User.IsDeleted, cancellationToken);

            var activeLearners = await _context.LearnerPackages
                .Select(lp => lp.LearnerId)
                .Distinct()
                .CountAsync(cancellationToken);

            var dto = new AdminFinancialOverviewDto
            {
                TotalGrossRevenue = grossRevenue,
                TotalPlatformCommissionEarned = platformCommission,
                TotalTeacherWalletsBalance = walletsAvailable,
                TotalTeacherEarningsPaid = walletsTotalEarned,
                TotalPackageHoursLiability = packageHoursLiability,
                TotalCompletedSessions = completedSessions,
                TotalActiveTeachers = activeTeachers,
                TotalActiveLearners = activeLearners
            };

            return Result<AdminFinancialOverviewDto>.Success(
                dto,
                _localizer[LocalizationKeys.AdminManagement.OverviewRetrievedSuccess].Value);
        }
    }
}

