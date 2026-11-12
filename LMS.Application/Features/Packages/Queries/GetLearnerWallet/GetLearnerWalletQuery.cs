using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Features.Sessions;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Queries.GetLearnerWallet
{
    public class GetLearnerWalletQuery : IRequest<Result<LearnerSessionsWalletDto>>
    {
    }

    public class GetLearnerWalletQueryHandler : IRequestHandler<GetLearnerWalletQuery, Result<LearnerSessionsWalletDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetLearnerWalletQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<LearnerSessionsWalletDto>> Handle(
            GetLearnerWalletQuery request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var defaultInstructorName = _localizer[LocalizationKeys.Packages.AllInstructors].Value;

            var packages = await _context.LearnerPackages
                .AsNoTracking()
                .Include(lp => lp.Instructor!)
                    .ThenInclude(i => i.User)
                .Where(lp => lp.LearnerId == learnerId)
                .OrderByDescending(lp => lp.CreatedAt)
                .Select(lp => new LearnerPackageDto
                {
                    Id = lp.Id,
                    InstructorId = lp.InstructorId,
                    InstructorFullName = lp.Instructor != null && lp.Instructor.User != null ? lp.Instructor.User.FullName : defaultInstructorName,
                    PackageName = lp.PackageName,
                    TotalHours = lp.TotalHours,
                    RemainingHours = lp.RemainingHours,
                    HourlyRateSnapshot = lp.HourlyRateSnapshot,
                    TotalPrice = lp.TotalPrice,
                    IsActive = lp.IsActive,
                    CreatedAt = lp.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var hasFreeTrial = await SessionsValidation.HasFreeTrialAsync(
                _context, learnerId, cancellationToken);

            var activePackages = packages.Where(p => p.IsActive && p.RemainingHours > 0).ToList();
            var totalSessions = activePackages.Sum(p => p.TotalHours);
            var remainingSessions = activePackages.Sum(p => p.RemainingHours);
            var activePlanName = activePackages.FirstOrDefault()?.PackageName;

            var wallet = new LearnerSessionsWalletDto
            {
                TotalSessions = totalSessions,
                RemainingSessions = remainingSessions,
                ActivePlanName = activePlanName,
                HasFreeTrialAvailable = hasFreeTrial,
                Packages = packages
            };

            return Result<LearnerSessionsWalletDto>.Success(
                wallet,
                _localizer[LocalizationKeys.Packages.PackagesRetrieved].Value);
        }
    }
}

