using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
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

            RuleFor(command => command.AvailabilityInstanceId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SlotIdRequired].Value);

            RuleFor(command => command.DurationMinutes)
                .Must(d => d == 25 || d == 50)
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
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var blocker = await SessionsValidation.FindBookingBlockerAsync(
                _context,
                command.InstructorId,
                learnerId,
                command.AvailabilityInstanceId,
                _scheduling.MinimumBreakMinutes,
                cancellationToken);

            if (blocker != BookingBlocker.None)
            {
                context.AddFailure(
                    nameof(CreateBookingCommand.AvailabilityInstanceId),
                    _localizer[BlockerKey(blocker), _scheduling.MinimumBreakMinutes].Value);
                return;
            }

            if (command.IsTrial)
            {
                var hasTrial = await _context.Sessions
                    .AnyAsync(s => s.Bookings.Any(b => b.LearnerId == learnerId)
                        && s.IsTrial
                        && s.Status != Domain.Enums.SessionStatus.Cancelled,
                        cancellationToken);

                if (hasTrial)
                {
                    context.AddFailure(
                        nameof(CreateBookingCommand.IsTrial),
                        _localizer[LocalizationKeys.Sessions.TrialAlreadyUsed].Value);
                }
            }
            else
            {
                var requiredHours = command.DurationMinutes == 25 ? 0.5m : 1.0m;
                var hasBalance = await _context.LearnerPackages
                    .AnyAsync(lp => lp.LearnerId == learnerId
                        && lp.InstructorId == command.InstructorId
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
        }

        private static string BlockerKey(BookingBlocker blocker) => blocker switch
        {
            BookingBlocker.InstructorNotBookable => LocalizationKeys.Sessions.InstructorNotBookable,
            BookingBlocker.InstanceMissing => LocalizationKeys.Sessions.SlotNotOpen,
            BookingBlocker.SlotElapsed => LocalizationKeys.Sessions.SlotElapsed,
            BookingBlocker.InstructorConflict => LocalizationKeys.Sessions.InstructorBusy,
            BookingBlocker.InstructorBreak => LocalizationKeys.Sessions.InstructorBreakTooShort,
            BookingBlocker.LearnerConflict => LocalizationKeys.Sessions.LearnerBusy,
            _ => LocalizationKeys.Sessions.LearnerBreakTooShort
        };
    }
}
