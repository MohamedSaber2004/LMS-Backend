using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.DeclineReschedule
{
    public class DeclineRescheduleCommandValidator : AbstractValidator<DeclineRescheduleCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeclineRescheduleCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.RescheduleIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeclineRescheduleCommand command,
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
                    nameof(DeclineRescheduleCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotPending].Value);
                return;
            }

            await SessionGuards.EnsureOtherSideAsync(
                _context, _currentUser, reschedule.Session, reschedule.RequestedByType, cancellationToken);
        }
    }
}
