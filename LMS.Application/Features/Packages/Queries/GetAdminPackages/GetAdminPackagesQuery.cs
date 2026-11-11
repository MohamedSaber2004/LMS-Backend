using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Queries.GetAdminPackages
{
    public class GetAdminPackagesQuery : IRequest<Result<PagginatedResult<AdminPackageItemDto>>>
    {
        public Guid? InstructorId { get; set; }
        public Guid? LearnerId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<AdminPackageItemDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<AdminPackageItemDto>.DefaultPageSize;
    }

    public class GetAdminPackagesQueryHandler : IRequestHandler<GetAdminPackagesQuery, Result<PagginatedResult<AdminPackageItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminPackagesQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<AdminPackageItemDto>>> Handle(
            GetAdminPackagesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.LearnerPackages
                .AsNoTracking()
                .Include(lp => lp.Learner)
                    .ThenInclude(l => l.User)
                .Include(lp => lp.Instructor!)
                    .ThenInclude(i => i.User)
                .AsQueryable();

            if (request.InstructorId.HasValue)
            {
                query = query.Where(lp => lp.InstructorId == request.InstructorId.Value);
            }

            if (request.LearnerId.HasValue)
            {
                query = query.Where(lp => lp.LearnerId == request.LearnerId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(lp => lp.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

            var items = await query
                .OrderByDescending(lp => lp.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(lp => new AdminPackageItemDto
                {
                    PackageId = lp.Id,
                    LearnerId = lp.LearnerId,
                    LearnerFullName = lp.Learner.User.FullName,
                    LearnerEmail = lp.Learner.User.Email ?? string.Empty,
                    InstructorId = lp.InstructorId,
                    InstructorFullName = lp.Instructor != null && lp.Instructor.User != null ? lp.Instructor.User.FullName : "All Instructors",
                    PackageName = lp.PackageName,
                    TotalHours = lp.TotalHours,
                    RemainingHours = lp.RemainingHours,
                    HourlyRateSnapshot = lp.HourlyRateSnapshot,
                    TotalPrice = lp.TotalPrice,
                    IsActive = lp.IsActive,
                    CreatedAtUtc = lp.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<PagginatedResult<AdminPackageItemDto>>.Success(
                PagginatedResult<AdminPackageItemDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.AdminManagement.PackagesRetrievedSuccess].Value);
        }
    }
}
