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

            if (command.Date == default)
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

            var pick = await SessionsValidation.PickBookableSlotOnDateAsync(
                _context,
                command.InstructorId,
                learnerId,
                command.Date,
                durationMinutes,
                _scheduling.MinimumBreakMinutes,
                cancellationToken);

            if (pick.Status != BookingSlotStatus.Bookable)
            {
                context.AddFailure(nameof(CreateBookingCommand.Date), UnavailableReason(pick, date, durationMinutes));
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

        private string UnavailableReason(BookingSlotPick pick, string date, int durationMinutes)
        {
            return pick.Status switch
            {
                BookingSlotStatus.InstructorNotBookable
                    => _localizer[LocalizationKeys.Sessions.InstructorNotBookable].Value,

                BookingSlotStatus.NoWindowsOnDate
                    => _localizer[LocalizationKeys.Sessions.NoAvailabilityOnDate, date].Value,

                BookingSlotStatus.AllWindowsElapsed
                    => _localizer[LocalizationKeys.Sessions.AllAvailabilityElapsed, date].Value,

                BookingSlotStatus.NoWindowFitsDuration
                    => _localizer[
                        LocalizationKeys.Sessions.NoWindowFitsDuration,
                        pick.WindowCount,
                        date,
                        durationMinutes].Value,

                _ => _localizer[
                    LocalizationKeys.Sessions.AllAvailabilityOnDateTaken,
                    pick.WindowCount,
                    date,
                    _localizer[BlockerKey(pick.Blocker), _scheduling.MinimumBreakMinutes].Value.TrimEnd('.')].Value
            };
        }

        private static string BlockerKey(BookingBlocker blocker) => blocker switch
        {
            BookingBlocker.InstructorConflict => LocalizationKeys.Sessions.InstructorBusy,
            BookingBlocker.InstructorBreak => LocalizationKeys.Sessions.InstructorBreakTooShort,
            BookingBlocker.LearnerConflict => LocalizationKeys.Sessions.LearnerBusy,
            _ => LocalizationKeys.Sessions.LearnerBreakTooShort
        };
    }
}
