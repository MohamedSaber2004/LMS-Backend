using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Sessions.Commands.CreateBooking
{
    public class CreateBookingCommandHandler
        : IRequestHandler<CreateBookingCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly SchedulingOptions _scheduling;

        public CreateBookingCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IOptions<SchedulingOptions> options,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _scheduling = options.Value;
        }

        public async Task<Result<SessionDto>> Handle(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.User)
                .FirstOrDefaultAsync(
                    item => item.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var durationMinutes = SessionsValidation.ResolveDurationMinutes(request.DurationMinutes);

            var pick = await SessionsValidation.ResolveRequestedWindowAsync(
                _context,
                request.InstructorId,
                learnerId,
                request.Date,
                request.StartTime!.Value,
                durationMinutes,
                _scheduling.MinimumBreakMinutes,
                cancellationToken);

            if (pick.Status != BookingSlotStatus.Bookable)
            {
                throw new NotFoundException(LocalizationKeys.Sessions.SlotNotFound);
            }

            var isTrial = await SessionsValidation.HasFreeTrialAsync(
                _context, learnerId, cancellationToken);
            decimal sessionPrice = 0m;

            if (!isTrial)
            {
                var requiredHours = durationMinutes == 25 ? 0.5m : 1.0m;
                var hasActivePackage = await _context.LearnerPackages
                    .AnyAsync(lp => lp.LearnerId == learnerId
                        && lp.IsActive
                        && lp.RemainingHours >= requiredHours,
                        cancellationToken);

                if (!hasActivePackage)
                {
                    throw new BadRequestException(LocalizationKeys.Sessions.InsufficientPackageHours);
                }

                sessionPrice = Math.Round(instructor.HourlyRate * requiredHours, 2);
            }

            var session = Session.Create(
                request.InstructorId,
                SessionType.OneToOne,
                pick.StartsAtUtc,
                pick.EndsAtUtc,
                sessionPrice,
                title: isTrial ? _localizer[LocalizationKeys.Sessions.FreeTrialLessonTitle].Value : null,
                seatCapacity: 1,
                lessonId: null,
                durationMinutes: durationMinutes,
                isTrial: isTrial,
                hourlyRateSnapshot: instructor.HourlyRate,
                commissionRateSnapshot: instructor.CommissionRate);

            var booking = SessionBooking.Create(session.Id, learnerId);

            booking.Confirm();

            if (isTrial)
            {
                session.Confirm();
            }

            session.ReserveSeat();

            var room = await _context.LiveRooms
                .FirstOrDefaultAsync(item => item.SessionId == session.Id, cancellationToken);

            if (room == null)
            {
                await _context.LiveRooms.AddAsync(
                    LiveRoom.Create(session.Id, instructor.UserId), cancellationToken);
            }

            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SessionBookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, instructor.User?.FullName ?? string.Empty, booking),
                _localizer[isTrial
                    ? LocalizationKeys.Sessions.BookingTrialReadySuccess
                    : LocalizationKeys.Sessions.BookingCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
