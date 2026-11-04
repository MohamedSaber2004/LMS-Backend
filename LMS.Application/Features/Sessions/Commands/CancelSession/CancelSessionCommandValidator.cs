using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Sessions.Commands.CancelSession
{
    public class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
    {
        private const int MaxReasonLength = 1000;

        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly SchedulingOptions _scheduling;

        public CancelSessionCommandValidator(
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

            RuleFor(command => command.Reason)
                .MaximumLength(MaxReasonLength)
                .WithMessage(localizer[LocalizationKeys.Sessions.ReasonTooLong, MaxReasonLength].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            CancelSessionCommand command,
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

            if (!SessionsValidation.IsLiveSession(session))
            {
                context.AddFailure(
                    nameof(CancelSessionCommand.Id),
                    _localizer[LocalizationKeys.Sessions.SessionNotCancellable].Value);
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
                    nameof(CancelSessionCommand.Id),
                    _localizer[actor == SessionActor.Teacher
                        ? LocalizationKeys.Sessions.InstructorLockPassed
                        : LocalizationKeys.Sessions.LearnerLockPassed,
                        lockHours].Value);
            }
        }
    }
}
