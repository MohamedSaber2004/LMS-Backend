using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.DeclineSession
{
    public class DeclineSessionCommandValidator : AbstractValidator<DeclineSessionCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeclineSessionCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SessionIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeclineSessionCommand command,
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

            await SessionGuards.EnsureTeacherAsync(_context, _currentUser, session, cancellationToken);

            if (session.Status != SessionStatus.Pending)
            {
                context.AddFailure(
                    nameof(DeclineSessionCommand.Id),
                    _localizer[LocalizationKeys.Sessions.SessionNotPending].Value);
            }
        }
    }
}
