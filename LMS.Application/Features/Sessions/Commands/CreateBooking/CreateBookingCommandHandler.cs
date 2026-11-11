using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.CreateBooking
{
    public class CreateBookingCommandHandler
        : IRequestHandler<CreateBookingCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateBookingCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.User)
                .FirstOrDefaultAsync(
                    item => item.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var instance = await _context.AvailabilityInstances
                .FirstOrDefaultAsync(
                    item => item.Id == request.AvailabilityInstanceId
                        && item.InstructorId == request.InstructorId,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SlotNotFound);

            var durationMinutes = request.DurationMinutes is 25 or 50 ? request.DurationMinutes : 50;
            var sessionEndsAtUtc = instance.StartsAtUtc.AddMinutes(durationMinutes);
            decimal sessionPrice = 0m;

            if (!request.IsTrial)
            {
                var requiredHours = durationMinutes == 25 ? 0.5m : 1.0m;
                var package = await _context.LearnerPackages
                    .Where(lp => lp.LearnerId == learnerId
                        && lp.IsActive
                        && lp.RemainingHours >= requiredHours)
                    .OrderBy(lp => lp.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new BadRequestException(LocalizationKeys.Sessions.InsufficientPackageHours);

                package.DeductHours(requiredHours);
                sessionPrice = Math.Round(instructor.HourlyRate * requiredHours, 2);
            }

            var session = Session.Create(
                request.InstructorId,
                SessionType.OneToOne,
                instance.StartsAtUtc,
                sessionEndsAtUtc,
                sessionPrice,
                title: request.IsTrial ? "Free Trial Lesson" : null,
                seatCapacity: 1,
                lessonId: null,
                durationMinutes: durationMinutes,
                isTrial: request.IsTrial,
                hourlyRateSnapshot: instructor.HourlyRate,
                commissionRateSnapshot: instructor.CommissionRate);

            var booking = SessionBooking.Create(session.Id, learnerId);

            session.ReserveSeat();

            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SessionBookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, instructor.User?.FullName ?? string.Empty, booking),
                _localizer[LocalizationKeys.Sessions.BookingCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
