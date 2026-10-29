using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
    {
        private const int FullNameMaxLength = 200;
        private const int PhoneNumberMaxLength = 30;

        private static readonly EmailAddressAttribute EmailValidator = new();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddUserCommandValidator(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IOptions<AppIdentityOptions> identityOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;

            var passwordMinLength = identityOptions?.Value?.RequiredLength > 0
                ? identityOptions.Value.RequiredLength
                : 8;

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameRequired].Value)
                .MaximumLength(FullNameMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameTooLong, FullNameMaxLength].Value);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.EmailRequired].Value)
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailValidator.IsValid(email))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidEmailFormat].Value);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordRequired].Value)
                .MinimumLength(passwordMinLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordTooShort, passwordMinLength].Value);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.PhoneNumberRequired].Value)
                .MaximumLength(PhoneNumberMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberValidationHelper.IsValid(phone))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value);

            RuleFor(x => x.CountryId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.CountryRequired].Value);

            RuleFor(x => x.Role)
                .Must(role => Enum.IsDefined(typeof(UserType), role))
                .WithMessage(localizer[LocalizationKeys.Users.InvalidRole].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            AddUserCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var email = command.Email?.Trim();
            var isEmailValid = !string.IsNullOrWhiteSpace(email) && EmailValidator.IsValid(email);

            if (isEmailValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(email!);
                existingUser ??= await _userManager.FindByNameAsync(email!);

                if (existingUser is not null)
                {
                    context.AddFailure(
                        nameof(AddUserCommand.Email),
                        _localizer[LocalizationKeys.Authentication.EmailAlreadyRegistered].Value);
                }
            }

            if (command.CountryId != Guid.Empty
                && !await _context.Countries.AnyAsync(
                    country => country.Id == command.CountryId, cancellationToken))
            {
                context.AddFailure(
                    nameof(AddUserCommand.CountryId),
                    _localizer[LocalizationKeys.Authentication.CountryNotFound].Value);
            }

            if (string.IsNullOrWhiteSpace(command.Password))
            {
                return;
            }

            var passwordProbe = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            if (!await AuthenticationValidationHelper.MeetsPasswordPolicyAsync(
                    _userManager,
                    passwordProbe,
                    command.Password))
            {
                context.AddFailure(
                    nameof(AddUserCommand.Password),
                    _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value);
            }
        }
    }
}
