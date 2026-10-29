using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Commands.AddCountry
{
    public class AddCountryCommandValidator : AbstractValidator<AddCountryCommand>
    {
        private const int NameMaxLength = 150;
        private const int CodeMaxLength = 20;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddCountryCommandValidator(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.CountryEnName)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Countries.CountryEnNameRequired].Value)
                .MaximumLength(NameMaxLength)
                .WithMessage(localizer[LocalizationKeys.Countries.CountryEnNameTooLong, NameMaxLength].Value);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Countries.CodeRequired].Value)
                .MaximumLength(CodeMaxLength)
                .WithMessage(localizer[LocalizationKeys.Countries.CodeTooLong, CodeMaxLength].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateUniquenessAsync);
        }

        private async Task ValidateUniquenessAsync(
            AddCountryCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var name = command.CountryEnName?.Trim();
            var code = command.Code?.Trim();

            if (!string.IsNullOrWhiteSpace(code)
                && await _context.Countries.AnyAsync(
                    country => country.Code == code, cancellationToken))
            {
                context.AddFailure(
                    nameof(AddCountryCommand.Code),
                    _localizer[LocalizationKeys.Countries.CountryCodeAlreadyExists].Value);
            }

            if (!string.IsNullOrWhiteSpace(name)
                && await _context.Countries.AnyAsync(
                    country => country.CountryEnName == name, cancellationToken))
            {
                context.AddFailure(
                    nameof(AddCountryCommand.CountryEnName),
                    _localizer[LocalizationKeys.Countries.CountryNameAlreadyExists].Value);
            }
        }
    }
}
