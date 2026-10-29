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

namespace LMS.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private const int FullNameMaxLength = 200;
        private const int PhoneNumberMaxLength = 30;

        private static readonly EmailAddressAttribute EmailValidator = new();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateUserCommandValidator(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Users.UserIdRequired].Value);

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameRequired].Value)
                .MaximumLength(FullNameMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameTooLong, FullNameMaxLength].Value)
                .When(x => x.FullName is not null);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.EmailRequired].Value)
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailValidator.IsValid(email))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidEmailFormat].Value)
                .When(x => x.Email is not null);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.PhoneNumberRequired].Value)
                .MaximumLength(PhoneNumberMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberValidationHelper.IsValid(phone))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value)
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.CountryId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.CountryRequired].Value)
                .When(x => x.CountryId.HasValue);

            RuleFor(x => x.Role)
                .Must(role => role.HasValue && Enum.IsDefined(typeof(UserType), role.Value))
                .WithMessage(localizer[LocalizationKeys.Users.InvalidRole].Value)
                .When(x => x.Role.HasValue);

            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage(localizer[LocalizationKeys.Users.NoFieldsToUpdate].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private static bool HasAtLeastOneField(UpdateUserCommand command)
        {
            return command.FullName is not null
                || command.Email is not null
                || command.PhoneNumber is not null
                || command.CountryId.HasValue
                || command.Role.HasValue
                || command.IsActive.HasValue;
        }

        private async Task ValidateAsync(
            UpdateUserCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(command.Id.ToString());

            if (user is null || user.IsDeleted)
            {
                context.AddFailure(
                    nameof(UpdateUserCommand.Id),
                    _localizer[LocalizationKeys.Users.UserNotFound].Value);
                return;
            }

            var email = command.Email?.Trim();

            if (!string.IsNullOrWhiteSpace(email) && EmailValidator.IsValid(email))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                existingUser ??= await _userManager.FindByNameAsync(email);

                if (existingUser is not null && existingUser.Id != command.Id)
                {
                    context.AddFailure(
                        nameof(UpdateUserCommand.Email),
                        _localizer[LocalizationKeys.Authentication.EmailAlreadyRegistered].Value);
                }
            }

            if (command.CountryId.HasValue
                && !await _context.Countries.AnyAsync(
                    country => country.Id == command.CountryId.Value, cancellationToken))
            {
                context.AddFailure(
                    nameof(UpdateUserCommand.CountryId),
                    _localizer[LocalizationKeys.Authentication.CountryNotFound].Value);
            }
        }
    }
}
