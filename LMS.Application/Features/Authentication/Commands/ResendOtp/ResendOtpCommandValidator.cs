using System.ComponentModel.DataAnnotations;
using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.ResendOtp
{
    public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
    {
        private static readonly EmailAddressAttribute EmailValidator = new();
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(30);

        public ResendOtpCommandValidator(
            UserManager<ApplicationUser> userManager,
            IOtpRateLimiter otpRateLimiter,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.EmailRequired].Value)
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailValidator.IsValid(email))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidEmailFormat].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.Email) || !EmailValidator.IsValid(command.Email))
                        return;

                    var normalizedEmail = command.Email.Trim().ToLowerInvariant();

                    var (isAllowed, remainingSeconds) = otpRateLimiter.CheckAndRecord(normalizedEmail, ResendCooldown);
                    if (!isAllowed)
                    {
                        throw new TooManyRequestsException(LocalizationKeys.Authentication.OtpRateLimited, remainingSeconds);
                    }

                    var user = await userManager.FindByEmailAsync(command.Email.Trim());
                    if (user == null || user.IsDeleted || !user.IsActive)
                    {
                        context.AddFailure(nameof(command.Email), localizer[LocalizationKeys.Authentication.UserNotFound].Value);
                        return;
                    }

                    if (await userManager.IsLockedOutAsync(user))
                    {
                        context.AddFailure(nameof(command.Email), localizer[LocalizationKeys.Authentication.AccountLocked].Value);
                        return;
                    }
                });
        }
    }
}
