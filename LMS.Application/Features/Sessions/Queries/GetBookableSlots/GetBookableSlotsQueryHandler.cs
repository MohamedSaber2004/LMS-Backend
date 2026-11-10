using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetBookableSlots
{
    public class GetBookableSlotsQueryHandler
        : IRequestHandler<GetBookableSlotsQuery, Result<IReadOnlyCollection<BookableSlotDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetBookableSlotsQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<BookableSlotDto>>> Handle(
            GetBookableSlotsQuery request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var slots = await _context.AvailabilityInstances
                .AsNoTracking()
                .Where(instance => instance.InstructorId == request.InstructorId
                    && instance.IsOpen
                    && instance.Date >= request.From
                    && instance.Date <= request.To
                    && instance.EndsAtUtc > now
                    && !_context.Sessions.Any(session => session.InstructorId == instance.InstructorId
                        && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                        && session.EndsAtUtc > now
                        && session.StartsAtUtc < instance.EndsAtUtc
                        && session.EndsAtUtc > instance.StartsAtUtc))
                .OrderBy(instance => instance.StartsAtUtc)
                .Select(instance => new BookableSlotDto
                {
                    AvailabilityInstanceId = instance.Id,
                    Date = instance.Date,
                    StartsAtUtc = instance.StartsAtUtc,
                    EndsAtUtc = instance.EndsAtUtc
                })
                .ToListAsync(cancellationToken);

            TimeZoneInfo? clientZone = null;
            if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
            {
                TimezoneHelper.TryResolve(request.TimeZoneId, out clientZone);
            }

            var resultSlots = slots.Select(slot =>
            {
                if (clientZone != null)
                {
                    slot.TimeZoneId = clientZone.Id;
                    slot.LocalStartsAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(slot.StartsAtUtc, DateTimeKind.Utc), clientZone);
                    slot.LocalEndsAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(slot.EndsAtUtc, DateTimeKind.Utc), clientZone);
                    slot.LocalDate = DateOnly.FromDateTime(slot.LocalStartsAt.Value);
                }
                return slot;
            }).ToList();

            return Result<IReadOnlyCollection<BookableSlotDto>>.Success(
                resultSlots,
                _localizer[LocalizationKeys.Sessions.SlotsRetrieved].Value);
        }
    }
}
