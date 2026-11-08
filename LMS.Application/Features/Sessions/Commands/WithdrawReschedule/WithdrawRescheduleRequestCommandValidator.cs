using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommandValidator : AbstractValidator<WithdrawRescheduleRequestCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public WithdrawRescheduleRequestCommandValidator(
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
            WithdrawRescheduleRequestCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .Include(r => r.Session)
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (reschedule is null)
            {
                return;
            }

            if (reschedule.Status != RescheduleRequestStatus.Pending)
            {
                context.AddFailure(
                    nameof(WithdrawRescheduleRequestCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotWithdrawable].Value);
                return;
            }

            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUser.UserId, cancellationToken);

            if (reschedule.Session.InstructorId != instructorId ||
                reschedule.RequestedByType != UserType.Instructor ||
                reschedule.RequestedByUserId != _currentUser.UserId)
            {
                context.AddFailure(
                    nameof(WithdrawRescheduleRequestCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotOwner].Value);
            }
        }
    }
}
