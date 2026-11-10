using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Queries.GetTeacherLearners
{
    public class GetTeacherLearnersQueryHandler : IRequestHandler<GetTeacherLearnersQuery, Result<PagginatedResult<TeacherLearnerItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetTeacherLearnersQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<TeacherLearnerItemDto>>> Handle(
            GetTeacherLearnersQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.UserId == _currentUserService.UserId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var packagesQuery = _context.LearnerPackages
                .AsNoTracking()
                .Include(lp => lp.Learner)
                    .ThenInclude(l => l.User)
                        .ThenInclude(u => u.Country)
                .Where(lp => lp.InstructorId == instructor.Id);

            var learnersMap = await packagesQuery
                .GroupBy(lp => lp.LearnerId)
                .Select(g => new
                {
                    LearnerId = g.Key,
                    Learner = g.First().Learner,
                    TotalPurchased = g.Sum(x => x.TotalHours),
                    Remaining = g.Sum(x => x.RemainingHours)
                })
                .ToListAsync(cancellationToken);

            var sessionsQuery = await _context.Sessions
                .AsNoTracking()
                .Where(s => s.InstructorId == instructor.Id)
                .SelectMany(s => s.Bookings.Select(b => new
                {
                    b.LearnerId,
                    s.Status,
                    s.StartsAtUtc
                }))
                .ToListAsync(cancellationToken);

            var sessionsGrouped = sessionsQuery
                .GroupBy(x => x.LearnerId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalCount = g.Count(),
                        CompletedCount = g.Count(x => x.Status == SessionStatus.Completed),
                        LastDate = g.OrderByDescending(x => x.StartsAtUtc).Select(x => (DateTime?)x.StartsAtUtc).FirstOrDefault()
                    });

            var allLearnerIds = learnersMap.Select(l => l.LearnerId)
                .Union(sessionsGrouped.Keys)
                .Distinct()
                .ToList();

            var learnersInfo = await _context.Learners
                .AsNoTracking()
                .Include(l => l.User)
                    .ThenInclude(u => u.Country)
                .Where(l => allLearnerIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, cancellationToken);

            var items = new List<TeacherLearnerItemDto>();
            foreach (var learnerId in allLearnerIds)
            {
                if (!learnersInfo.TryGetValue(learnerId, out var learner))
                    continue;

                var user = learner.User;
                var pkg = learnersMap.FirstOrDefault(p => p.LearnerId == learnerId);
                sessionsGrouped.TryGetValue(learnerId, out var sess);

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var matchName = user.FullName.Contains(request.Search, StringComparison.OrdinalIgnoreCase);
                    var matchEmail = user.Email != null && user.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase);
                    if (!matchName && !matchEmail) continue;
                }

                items.Add(new TeacherLearnerItemDto
                {
                    LearnerId = learnerId,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Country = user.Country?.CountryEnName,
                    TotalHoursPurchased = pkg?.TotalPurchased ?? 0,
                    RemainingHours = pkg?.Remaining ?? 0,
                    TotalSessionsCount = sess?.TotalCount ?? 0,
                    CompletedSessionsCount = sess?.CompletedCount ?? 0,
                    LastSessionDateUtc = sess?.LastDate
                });
            }

            var totalCount = items.Count;
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

            var paged = items
                .OrderByDescending(x => x.LastSessionDateUtc ?? DateTime.MinValue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Result<PagginatedResult<TeacherLearnerItemDto>>.Success(
                PagginatedResult<TeacherLearnerItemDto>.Create(paged, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.TeacherManagement.LearnersRetrievedSuccess].Value);
        }
    }
}
