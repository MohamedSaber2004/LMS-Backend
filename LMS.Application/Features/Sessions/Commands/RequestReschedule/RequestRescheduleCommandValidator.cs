using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Sessions.Commands.RequestReschedule
{
    public class RequestRescheduleCommandValidator : AbstractValidator<RequestRescheduleCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly SchedulingOptions _scheduling;

        public RequestRescheduleCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IOptions<SchedulingOptions> options,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;
            _scheduling = options.Value;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SessionIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            RequestRescheduleCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken);

            if (session is null)
            {
                // A missing row is reported as 404 by the handler.
                return;
            }

            if (command.FromUtc <= DateTime.UtcNow || command.ToUtc <= command.FromUtc)
            {
                context.AddFailure(
                    nameof(RequestRescheduleCommand.FromUtc),
                    _localizer[LocalizationKeys.Sessions.RescheduleWindowInvalid].Value);
                return;
            }

            if (!SessionsValidation.IsLiveSession(session))
            {
                context.AddFailure(
                    nameof(RequestRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.SessionNotReschedulable].Value);
                return;
            }

            if (!await SessionsValidation.HasNoPendingRescheduleAsync(_context, session.Id, cancellationToken))
            {
                context.AddFailure(
                    nameof(RequestRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleAlreadyPending].Value);
                return;
            }

            var actor = await SessionGuards.ResolveActorAsync(
                _context, _currentUser, session, cancellationToken);

            var lockHours = actor == SessionActor.Teacher
                ? _scheduling.InstructorLockHours
                : _scheduling.LearnerLockHours;

            if (!SessionsValidation.IsOutsideLock(session.StartsAtUtc, lockHours))
            {
                context.AddFailure(
                    nameof(RequestRescheduleCommand.Id),
                    _localizer[actor == SessionActor.Teacher
                        ? LocalizationKeys.Sessions.InstructorLockPassed
                        : LocalizationKeys.Sessions.LearnerLockPassed,
                        lockHours].Value);
                return;
            }

            // The lesson stays the same length: only its position on the timeline moves.
            if (command.ToUtc - command.FromUtc != session.EndsAtUtc - session.StartsAtUtc)
            {
                context.AddFailure(
                    nameof(RequestRescheduleCommand.ToUtc),
                    _localizer[LocalizationKeys.Sessions.RescheduleDurationChanged].Value);
            }
        }
    }
}
