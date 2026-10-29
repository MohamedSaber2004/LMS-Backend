using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Helpers;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;

        public ChangeUserPasswordCommandValidator(
            UserManager<ApplicationUser> userManager,
            IOptions<AppIdentityOptions> identityOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _localizer = localizer;

            var requiredLength = identityOptions?.Value?.RequiredLength > 0
                ? identityOptions.Value.RequiredLength
                : 8;

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Users.UserIdRequired].Value);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordRequired].Value)
                .MinimumLength(requiredLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordTooShort, requiredLength].Value);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordRequired].Value)
                .Equal(x => x.NewPassword)
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordMismatch].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            ChangeUserPasswordCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.UserId == Guid.Empty)
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(command.UserId.ToString());

            if (user is null || user.IsDeleted)
            {
                context.AddFailure(
                    nameof(ChangeUserPasswordCommand.UserId),
                    _localizer[LocalizationKeys.Users.UserNotFound].Value);
                return;
            }

            if (string.IsNullOrWhiteSpace(command.NewPassword)
                || command.NewPassword != command.ConfirmNewPassword)
            {
                return;
            }

            if (!await AuthenticationValidationHelper.MeetsPasswordPolicyAsync(
                    _userManager,
                    user,
                    command.NewPassword))
            {
                context.AddFailure(
                    nameof(ChangeUserPasswordCommand.NewPassword),
                    _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value);
            }
        }
    }
}
