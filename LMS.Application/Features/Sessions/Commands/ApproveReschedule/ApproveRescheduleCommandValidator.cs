using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Sessions.Commands.ApproveReschedule
{
    public class ApproveRescheduleCommandValidator : AbstractValidator<ApproveRescheduleCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly SchedulingOptions _scheduling;

        public ApproveRescheduleCommandValidator(
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
                .WithMessage(localizer[LocalizationKeys.Sessions.RescheduleIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            ApproveRescheduleCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .Include(item => item.Session)
                .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken);

            if (reschedule is null)
            {
                // A missing row is reported as 404 by the handler.
                return;
            }

            if (reschedule.Status != RescheduleRequestStatus.Pending)
            {
                context.AddFailure(
                    nameof(ApproveRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotPending].Value);
                return;
            }

            var session = reschedule.Session;

            if (!SessionsValidation.IsLiveSession(session))
            {
                context.AddFailure(
                    nameof(ApproveRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.SessionNotReschedulable].Value);
                return;
            }

            await SessionGuards.EnsureOtherSideAsync(
                _context, _currentUser, session, reschedule.RequestedByType, cancellationToken);

            if (reschedule.FromUtc <= DateTime.UtcNow)
            {
                context.AddFailure(
                    nameof(ApproveRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleWindowInvalid].Value);
                return;
            }

            if (!await SessionsValidation.WindowIsInsideAvailabilityAsync(
                    _context, session.InstructorId, reschedule.FromUtc, reschedule.ToUtc, cancellationToken)
                || !await SessionsValidation.NewWindowIsFreeAsync(
                    _context,
                    session.Id,
                    session.InstructorId,
                    reschedule.FromUtc,
                    reschedule.ToUtc,
                    _scheduling.MinimumBreakMinutes,
                    cancellationToken))
            {
                context.AddFailure(
                    nameof(ApproveRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleConflicted].Value);
            }
        }
    }
}
