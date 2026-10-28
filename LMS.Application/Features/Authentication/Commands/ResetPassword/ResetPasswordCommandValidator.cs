using System.ComponentModel.DataAnnotations;
using FluentValidation;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        private static readonly EmailAddressAttribute EmailValidator = new();

        public ResetPasswordCommandValidator(
            UserManager<ApplicationUser> userManager,
            IOptions<AppIdentityOptions> identityOptions,
            IStringLocalizer<Messages> localizer)
        {
            var requiredLength = identityOptions?.Value?.RequiredLength > 0
                ? identityOptions.Value.RequiredLength
                : 8;

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.EmailRequired].Value)
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailValidator.IsValid(email))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidEmailFormat].Value);

            RuleFor(x => x.Otp)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.OtpRequired].Value)
                .Length(6)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidOtpFormat].Value)
                .Matches("^[0-9]+$")
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidOtpFormat].Value);

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
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.Email)
                        || !EmailValidator.IsValid(command.Email)
                        || string.IsNullOrWhiteSpace(command.Otp)
                        || command.Otp.Length != 6
                        || string.IsNullOrWhiteSpace(command.NewPassword)
                        || command.NewPassword != command.ConfirmNewPassword)
                    {
                        return;
                    }

                    var user = await userManager.FindByEmailAsync(command.Email.Trim());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        context.AddFailure(
                            nameof(command.Email),
                            localizer[LocalizationKeys.Authentication.UserNotFound].Value);
                        return;
                    }

                    if (!AuthenticationValidationHelper.IsOtpValid(user.VerificationCode, user.VerificationCodeExpiry, command.Otp))
                    {
                        context.AddFailure(
                            nameof(command.Otp),
                            localizer[LocalizationKeys.Authentication.OtpInvalidOrExpired].Value);
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
