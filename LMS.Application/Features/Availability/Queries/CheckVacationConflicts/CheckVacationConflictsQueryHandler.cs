using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQueryHandler
        : IRequestHandler<CheckVacationConflictsQuery, Result<VacationConflictResultDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CheckVacationConflictsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<VacationConflictResultDto>> Handle(
            CheckVacationConflictsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var startThreshold = request.FromUtc ?? DateTime.UtcNow;

            var query = _context.Sessions
                .AsNoTracking()
                .Include(s => s.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Where(s => s.InstructorId == instructorId
                    && (s.Status == SessionStatus.Confirmed || s.Status == SessionStatus.Pending)
                    && s.EndsAtUtc > startThreshold);

            if (request.ToUtc.HasValue)
            {
                query = query.Where(s => s.StartsAtUtc < request.ToUtc.Value);
            }

            var conflictedSessions = await query
                .OrderBy(s => s.StartsAtUtc)
                .Select(s => new VacationConflictedSessionDto
                {
                    SessionId = s.Id,
                    Title = s.Title,
                    StartsAtUtc = s.StartsAtUtc,
                    EndsAtUtc = s.EndsAtUtc,
                    Status = s.Status,
                    Type = s.Type,
                    SeatsBooked = s.SeatsBooked,
                    SeatCapacity = s.SeatCapacity,
                    LearnerNames = s.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                        .Select(b => b.Learner.User.FullName)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var result = new VacationConflictResultDto
            {
                ConflictedSessions = conflictedSessions
            };

            return Result<VacationConflictResultDto>.Success(
                result,
                _localizer[LocalizationKeys.Availability.VacationConflictsChecked].Value);
        }
    }
}
