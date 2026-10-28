using System.ComponentModel.DataAnnotations;
using FluentValidation;
using LMS.Application.Common.Helpers;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        private static readonly EmailAddressAttribute EmailValidator = new();

        public VerifyOtpCommandValidator(
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
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

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.Email)
                        || !EmailValidator.IsValid(command.Email)
                        || string.IsNullOrWhiteSpace(command.Otp)
                        || command.Otp.Length != 6)
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
                    }
                });
        }
    }
}
