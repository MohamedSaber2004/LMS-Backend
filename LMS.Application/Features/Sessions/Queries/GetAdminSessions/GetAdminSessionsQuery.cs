using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetAdminSessions
{
    public class GetAdminSessionsQuery : IRequest<Result<PagginatedResult<AdminSessionListItemDto>>>
    {
        public SessionStatus? Status { get; set; }
        public Guid? InstructorId { get; set; }
        public Guid? LearnerId { get; set; }
        public bool? IsTrial { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<AdminSessionListItemDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<AdminSessionListItemDto>.DefaultPageSize;
    }

    public class GetAdminSessionsQueryHandler : IRequestHandler<GetAdminSessionsQuery, Result<PagginatedResult<AdminSessionListItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminSessionsQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<AdminSessionListItemDto>>> Handle(
            GetAdminSessionsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Sessions
                .AsNoTracking()
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(s => s.Status == request.Status.Value);
            }

            if (request.InstructorId.HasValue)
            {
                query = query.Where(s => s.InstructorId == request.InstructorId.Value);
            }

            if (request.LearnerId.HasValue)
            {
                query = query.Where(s => s.Bookings.Any(b => b.LearnerId == request.LearnerId.Value));
            }

            if (request.IsTrial.HasValue)
            {
                query = query.Where(s => s.IsTrial == request.IsTrial.Value);
            }

            if (request.FromUtc.HasValue)
            {
                query = query.Where(s => s.StartsAtUtc >= request.FromUtc.Value);
            }

            if (request.ToUtc.HasValue)
            {
                query = query.Where(s => s.EndsAtUtc <= request.ToUtc.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

            var sessions = await query
                .OrderByDescending(s => s.StartsAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = sessions.Select(s =>
            {
                var booking = s.Bookings.FirstOrDefault();
                return new AdminSessionListItemDto
                {
                    SessionId = s.Id,
                    InstructorId = s.InstructorId,
                    InstructorFullName = s.Instructor.User?.FullName ?? string.Empty,
                    InstructorEmail = s.Instructor.User?.Email ?? string.Empty,
                    LearnerId = booking?.LearnerId,
                    LearnerFullName = booking?.Learner?.User?.FullName ?? string.Empty,
                    LearnerEmail = booking?.Learner?.User?.Email ?? string.Empty,
                    StartsAtUtc = s.StartsAtUtc,
                    EndsAtUtc = s.EndsAtUtc,
                    DurationMinutes = s.DurationMinutes,
                    IsTrial = s.IsTrial,
                    Status = s.Status,
                    IsAttendanceConfirmed = s.IsAttendanceConfirmed,
                    HourlyRateSnapshot = s.HourlyRateSnapshot,
                    CommissionRateSnapshot = s.CommissionRateSnapshot,
                    CreatedAtUtc = s.CreatedAt
                };
            }).ToList();

            return Result<PagginatedResult<AdminSessionListItemDto>>.Success(
                PagginatedResult<AdminSessionListItemDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.AdminManagement.SessionsRetrievedSuccess].Value);
        }
    }
}

