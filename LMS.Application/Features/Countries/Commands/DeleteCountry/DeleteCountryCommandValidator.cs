using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Commands.DeleteCountry
{
    public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteCountryCommandValidator(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Countries.CountryIdRequired].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeleteCountryCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var countryExists = await _context.Countries
                .AnyAsync(country => country.Id == command.Id, cancellationToken);

            if (!countryExists)
            {
                context.AddFailure(
                    nameof(DeleteCountryCommand.Id),
                    _localizer[LocalizationKeys.Countries.CountryNotFound].Value);
                return;
            }

            var countryInUse = await _context.ApplicationUsers
                .AnyAsync(user => user.CountryId == command.Id, cancellationToken);

            if (countryInUse)
            {
                context.AddFailure(
                    nameof(DeleteCountryCommand.Id),
                    _localizer[LocalizationKeys.Countries.CannotDeleteCountryInUse].Value);
            }
        }
    }
}
