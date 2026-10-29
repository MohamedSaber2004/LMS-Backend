using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteUserCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Users.UserIdRequired].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeleteUserCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var userExists = await _context.ApplicationUsers
                .AnyAsync(user => user.Id == command.Id && !user.IsDeleted, cancellationToken);

            if (!userExists)
            {
                context.AddFailure(
                    nameof(DeleteUserCommand.Id),
                    _localizer[LocalizationKeys.Users.UserNotFound].Value);
                return;
            }

            if (_currentUserService.UserId == command.Id)
            {
                context.AddFailure(
                    nameof(DeleteUserCommand.Id),
                    _localizer[LocalizationKeys.Users.CannotDeleteSelf].Value);
            }
        }
    }
}
