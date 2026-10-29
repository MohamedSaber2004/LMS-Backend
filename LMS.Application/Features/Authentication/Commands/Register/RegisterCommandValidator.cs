using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private const int FullNameMaxLength = 200;
        private const int PhoneNumberMaxLength = 30;
        private const int PhoneNumberMinDigits = 6;
        private const int PhoneNumberMaxDigits = 15;
        private const int SpecializationMaxLength = 150;
        private const int BioMaxLength = 2000;
        private const int LinkedInUrlMaxLength = 500;
        private const int MinYearsOfExperience = 0;
        private const int MaxYearsOfExperience = 60;

        private static readonly EmailAddressAttribute EmailValidator = new();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly int _passwordMinLength;

        public RegisterCommandValidator(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IFileValidator fileValidator,
            IOptions<AppIdentityOptions> identityOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _fileValidator = fileValidator;
            _localizer = localizer;
            _passwordMinLength = identityOptions?.Value?.RequiredLength > 0
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
                .MinimumLength(_passwordMinLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordTooShort, _passwordMinLength].Value);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.ConfirmPasswordRequired].Value)
                .Equal(x => x.Password)
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordsDoNotMatch].Value);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.PhoneNumberRequired].Value)
                .MaximumLength(PhoneNumberMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || BeValidPhoneNumber(phone))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidPhoneNumber].Value);

            RuleFor(x => x.CountryId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.CountryRequired].Value);

            RuleFor(x => x.Role)
                .Must(role => role == UserType.Learner || role == UserType.Instructor)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidRegistrationRole].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
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
                                nameof(RegisterCommand.Email),
                                _localizer[LocalizationKeys.Authentication.EmailAlreadyRegistered].Value);
                        }
                    }

                    if (command.CountryId != Guid.Empty)
                    {
                        var countryExists = await _context.Countries
                            .AnyAsync(country => country.Id == command.CountryId, cancellationToken);
                        if (!countryExists)
                        {
                            context.AddFailure(
                                nameof(RegisterCommand.CountryId),
                                _localizer[LocalizationKeys.Authentication.CountryNotFound].Value);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(command.Password)
                        || string.IsNullOrWhiteSpace(command.ConfirmPassword)
                        || command.Password != command.ConfirmPassword)
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
                            nameof(RegisterCommand.Password),
                            _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value);
                    }
                });

            When(x => x.Role == UserType.Instructor, () =>
            {
                RuleFor(x => x.Specialization)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Authentication.SpecializationRequired].Value)
                    .MaximumLength(SpecializationMaxLength)
                    .WithMessage(localizer[LocalizationKeys.Authentication.SpecializationTooLong, SpecializationMaxLength].Value);

                RuleFor(x => x.Bio)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Authentication.BioRequired].Value)
                    .MaximumLength(BioMaxLength)
                    .WithMessage(localizer[LocalizationKeys.Authentication.BioTooLong, BioMaxLength].Value);

                RuleFor(x => x.YearsOfExperience)
                    .NotNull()
                    .WithMessage(localizer[LocalizationKeys.Authentication.YearsOfExperienceRequired].Value)
                    .Must(value => value.HasValue
                        && value.Value >= MinYearsOfExperience
                        && value.Value <= MaxYearsOfExperience)
                    .WithMessage(localizer[
                        LocalizationKeys.Authentication.InvalidYearsOfExperience,
                        MinYearsOfExperience,
                        MaxYearsOfExperience].Value);

                RuleFor(x => x.LinkedInUrl)
                    .MaximumLength(LinkedInUrlMaxLength)
                    .WithMessage(localizer[LocalizationKeys.Authentication.LinkedInUrlTooLong, LinkedInUrlMaxLength].Value)
                    .Must(BeValidUrl)
                    .WithMessage(localizer[LocalizationKeys.Authentication.InvalidLinkedInUrl].Value)
                    .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));

                RuleFor(x => x.Certificates)
                    .Custom(ValidateCertificates);
            });
        }

        private void ValidateCertificates(List<IFormFile>? files, CustomContext context)
        {
            if (files is null || files.Count == 0)
            {
                context.AddFailure(
                    nameof(RegisterCommand.Certificates),
                    _localizer[LocalizationKeys.Authentication.CertificatesRequired].Value);
                return;
            }

            if (files.Any(file => !_fileValidator.IsValidFile(file)))
            {
                context.AddFailure(
                    nameof(RegisterCommand.Certificates),
                    _localizer[LocalizationKeys.Authentication.InvalidCertificateFile].Value);
            }
        }

        private static bool BeValidPhoneNumber(string phone)
        {
            var value = phone.Trim();

            if (value.Length > PhoneNumberMaxLength)
            {
                return false;
            }

            var plusIndex = value.IndexOf('+');
            if (plusIndex > 0 || (plusIndex == 0 && value.IndexOf('+', 1) >= 0))
            {
                return false;
            }

            foreach (var character in value)
            {
                if (!char.IsDigit(character)
                    && !char.IsWhiteSpace(character)
                    && character != '+'
                    && character != '-'
                    && character != '('
                    && character != ')'
                    && character != '.'
                    && character != '/')
                {
                    return false;
                }
            }

            var digitCount = value.Count(char.IsDigit);
            return digitCount >= PhoneNumberMinDigits && digitCount <= PhoneNumberMaxDigits;
        }

        private static bool BeValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
