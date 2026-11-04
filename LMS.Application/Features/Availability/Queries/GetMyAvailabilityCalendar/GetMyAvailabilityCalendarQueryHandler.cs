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

namespace LMS.Application.Features.Availability.Queries.GetMyAvailabilityCalendar
{
    public class GetMyAvailabilityCalendarQueryHandler
        : IRequestHandler<GetMyAvailabilityCalendarQuery, Result<AvailabilityCalendarDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyAvailabilityCalendarQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<AvailabilityCalendarDto>> Handle(
            GetMyAvailabilityCalendarQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                _context, instructorId, cancellationToken);

            var hasZone = TimezoneHelper.TryResolve(timezoneId, out var resolvedZone) && resolvedZone is not null;

            var instances = await _context.AvailabilityInstances
                .AsNoTracking()
                .Where(instance => instance.InstructorId == instructorId
                    && instance.Date >= request.From
                    && instance.Date <= request.To)
                .OrderBy(instance => instance.Date)
                .ThenBy(instance => instance.StartsAtUtc)
                .Select(instance => new
                {
                    instance.Id,
                    instance.Date,
                    instance.IsOpen,
                    instance.StartsAtUtc,
                    instance.EndsAtUtc
                })
                .ToListAsync(cancellationToken);

            List<SessionWindow> booked = new();

            if (hasZone
                && TimezoneHelper.TryToUtcDayBounds(request.From, resolvedZone!, out var rangeStartUtc, out _)
                && TimezoneHelper.TryToUtcDayBounds(request.To, resolvedZone!, out _, out var rangeEndUtc))
            {
                var sessions = await _context.Sessions
                    .AsNoTracking()
                    .Where(session => session.InstructorId == instructorId
                        && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                        && session.StartsAtUtc < rangeEndUtc
                        && session.EndsAtUtc > rangeStartUtc)
                    .OrderBy(session => session.StartsAtUtc)
                    .Select(session => new { session.Id, session.StartsAtUtc, session.EndsAtUtc })
                    .ToListAsync(cancellationToken);

                foreach (var session in sessions)
                {
                    TimezoneHelper.ToWallClock(session.StartsAtUtc, resolvedZone!, out var localDate, out _);

                    booked.Add(new SessionWindow(localDate, session.Id, session.StartsAtUtc, session.EndsAtUtc));
                }
            }

            var days = new List<AvailabilityDayDto>();

            for (var date = request.From; date <= request.To; date = date.AddDays(1))
            {
                days.Add(new AvailabilityDayDto
                {
                    Date = date,
                    Available = instances
                        .Where(instance => instance.Date == date && instance.IsOpen)
                        .Select(instance => ToWindow(instance.Id, instance.StartsAtUtc, instance.EndsAtUtc))
                        .ToArray(),
                    Blocked = instances
                        .Where(instance => instance.Date == date && !instance.IsOpen)
                        .Select(instance => ToWindow(instance.Id, instance.StartsAtUtc, instance.EndsAtUtc))
                        .ToArray(),
                    Booked = booked
                        .Where(session => session.Date == date)
                        .Select(session => ToWindow(session.Id, session.StartsAtUtc, session.EndsAtUtc))
                        .ToArray()
                });
            }

            var response = new AvailabilityCalendarDto
            {
                From = request.From,
                To = request.To,
                TimezoneId = timezoneId,
                Days = days
            };

            return Result<AvailabilityCalendarDto>.Success(
                response,
                _localizer[LocalizationKeys.Availability.CalendarRetrieved].Value);
        }

        private static AvailabilityWindowDto ToWindow(Guid id, DateTime startsAtUtc, DateTime endsAtUtc) => new()
        {
            Id = id,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc
        };

        private sealed record SessionWindow(DateOnly Date, Guid Id, DateTime StartsAtUtc, DateTime EndsAtUtc);
    }
}
