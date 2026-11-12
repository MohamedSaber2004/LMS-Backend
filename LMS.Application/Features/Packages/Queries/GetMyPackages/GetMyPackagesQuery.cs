using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Queries.GetMyPackages
{
    public class GetMyPackagesQuery : IRequest<Result<IReadOnlyCollection<LearnerPackageDto>>>
    {
        public Guid? InstructorId { get; set; }
    }

    public class GetMyPackagesQueryHandler : IRequestHandler<GetMyPackagesQuery, Result<IReadOnlyCollection<LearnerPackageDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyPackagesQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<LearnerPackageDto>>> Handle(
            GetMyPackagesQuery request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var query = _context.LearnerPackages
                .AsNoTracking()
                .Include(lp => lp.Instructor!)
                    .ThenInclude(i => i.User)
                .Where(lp => lp.LearnerId == learnerId);

            if (request.InstructorId.HasValue)
            {
                query = query.Where(lp => lp.InstructorId == request.InstructorId.Value);
            }

            var defaultInstructorName = _localizer[LocalizationKeys.Packages.AllInstructors].Value;

            var packages = await query
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

            return Result<IReadOnlyCollection<LearnerPackageDto>>.Success(
                packages,
                _localizer[LocalizationKeys.Packages.PackagesRetrieved].Value);
        }
    }
}

