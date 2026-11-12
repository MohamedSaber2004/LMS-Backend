using System.Globalization;
using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Features.Availability;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Sessions.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly SchedulingOptions _scheduling;

        public CreateBookingCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IOptions<SchedulingOptions> options,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;
            _scheduling = options.Value;

            RuleFor(command => command.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.InstructorIdRequired].Value);

            RuleFor(command => command.Date)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.DateRequired].Value);

            RuleFor(command => command.StartTime)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithMessage(localizer[LocalizationKeys.Sessions.StartTimeRequired].Value)
                .GreaterThanOrEqualTo(TimeSpan.Zero)
                .LessThan(TimeSpan.FromHours(24))
                .WithMessage(localizer[LocalizationKeys.Sessions.StartTimeInvalid].Value);

            RuleFor(command => command.DurationMinutes)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Sessions.DurationInvalid].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            CreateBookingCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUser.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUser.UserId);

            if (command.Date == default || command.StartTime is null)
            {
                return;
            }

            if (!await AvailabilityValidation.InstructorExistsAsync(_context, command.InstructorId, cancellationToken))
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.InstructorId),
                    _localizer[LocalizationKeys.Availability.InstructorNotFound].Value);
                return;
            }

            var zone = await AvailabilityValidation.ResolveInstructorZoneAsync(
                _context, command.InstructorId, cancellationToken);

            if (zone is null)
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.InstructorId),
                    _localizer[LocalizationKeys.Availability.TimezoneUnresolved].Value);
                return;
            }

            if (command.Date < TimezoneHelper.LocalToday(zone))
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.Date),
                    _localizer[LocalizationKeys.Availability.DateInPast].Value);
                return;
            }

            var date = command.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var durationMinutes = SessionsValidation.ResolveDurationMinutes(command.DurationMinutes);

            var pick = await SessionsValidation.ResolveRequestedWindowAsync(
                _context,
                command.InstructorId,
                learnerId,
                command.Date,
                command.StartTime.Value,
                durationMinutes,
                _scheduling.MinimumBreakMinutes,
                cancellationToken);

            if (pick.Status != BookingSlotStatus.Bookable)
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.StartTime),
                    UnavailableReason(pick, zone, date, command.StartTime.Value, durationMinutes));
                return;
            }

            if (await SessionsValidation.HasFreeTrialAsync(_context, learnerId, cancellationToken))
            {
                return;
            }

            var requiredHours = durationMinutes == 25 ? 0.5m : 1.0m;
            var hasBalance = await _context.LearnerPackages
                .AnyAsync(lp => lp.LearnerId == learnerId
                    && lp.IsActive
                    && lp.RemainingHours >= requiredHours,
                    cancellationToken);

            if (!hasBalance)
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.DurationMinutes),
                    _localizer[LocalizationKeys.Sessions.InsufficientPackageHours].Value);
            }
        }

        private string UnavailableReason(
            BookingSlotPick pick,
            TimeZoneInfo zone,
            string date,
            TimeSpan requestedStartTime,
            int durationMinutes)
        {
            var requested = requestedStartTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

            return pick.Status switch
            {
                BookingSlotStatus.InstructorNotBookable
                    => _localizer[LocalizationKeys.Sessions.InstructorNotBookable].Value,

                BookingSlotStatus.NoWindowsOnDate
                    => _localizer[LocalizationKeys.Sessions.NoAvailabilityOnDate, date].Value,

                BookingSlotStatus.TimezoneUnresolved
                    => _localizer[LocalizationKeys.Availability.TimezoneUnresolved].Value,

                BookingSlotStatus.StartTimeNotInTimeZone
                    => _localizer[LocalizationKeys.Sessions.StartTimeNotInTimeZone, requested, date].Value,

                BookingSlotStatus.StartTimeElapsed
                    => _localizer[LocalizationKeys.Sessions.StartTimeElapsed, requested, date].Value,

                BookingSlotStatus.TimeOutsideAvailability
                    => _localizer[
                        LocalizationKeys.Sessions.TimeOutsideAvailability,
                        requested,
                        date,
                        LocalTime(pick.FirstWindowStartsAtUtc, zone),
                        LocalTime(pick.LastWindowEndsAtUtc, zone),
                        durationMinutes].Value,

                _ => _localizer[
                    LocalizationKeys.Sessions.RequestedTimeBlocked,
                    requested,
                    date,
                    _localizer[BlockerKey(pick.Blocker), _scheduling.MinimumBreakMinutes].Value.TrimEnd('.')].Value
            };
        }

        private static string LocalTime(DateTime utcInstant, TimeZoneInfo zone)
            => TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc), zone)
                .ToString("HH:mm", CultureInfo.InvariantCulture);

        private static string BlockerKey(BookingBlocker blocker) => blocker switch
        {
            BookingBlocker.InstructorConflict => LocalizationKeys.Sessions.InstructorBusy,
            BookingBlocker.InstructorBreak => LocalizationKeys.Sessions.InstructorBreakTooShort,
            BookingBlocker.LearnerConflict => LocalizationKeys.Sessions.LearnerBusy,
            _ => LocalizationKeys.Sessions.LearnerBreakTooShort
        };
    }
}
