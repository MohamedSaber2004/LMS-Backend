using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Authentication.Commands.ChangetPassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IOptions<AppIdentityOptions> identityOptions,
            IStringLocalizer<Messages> localizer)
        {
            var requiredLength = identityOptions?.Value?.RequiredLength > 0
                ? identityOptions.Value.RequiredLength
                : 8;

            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.CurrentPasswordRequired].Value);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordRequired].Value)
                .MinimumLength(requiredLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordTooShort, requiredLength].Value)
                .NotEqual(x => x.CurrentPassword)
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordSameAsCurrent].Value);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordRequired].Value)
                .Equal(x => x.NewPassword)
                .WithMessage(localizer[LocalizationKeys.Authentication.NewPasswordMismatch].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.CurrentPassword)
                        || string.IsNullOrWhiteSpace(command.NewPassword)
                        || command.NewPassword != command.ConfirmNewPassword
                        || command.NewPassword == command.CurrentPassword)
                    {
                        return;
                    }

                    var user = await userManager.FindByIdAsync(currentUserService.UserId.ToString());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }

                    if (!await userManager.CheckPasswordAsync(user, command.CurrentPassword))
                    {
                        context.AddFailure(
                            nameof(command.CurrentPassword),
                            localizer[LocalizationKeys.Authentication.CurrentPasswordIncorrect].Value);
                        return;
                    }

                    if (!await AuthenticationValidationHelper.MeetsPasswordPolicyAsync(userManager, user, command.NewPassword))
                    {
                        context.AddFailure(
                            nameof(command.NewPassword),
                            localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value);
                    }
                });
        }
    }
}
