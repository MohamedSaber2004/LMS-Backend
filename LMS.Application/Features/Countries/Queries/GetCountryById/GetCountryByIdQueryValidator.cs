using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQueryValidator : AbstractValidator<GetCountryByIdQuery>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetCountryByIdQueryValidator(ILmsDbContext context, IStringLocalizer<Messages> localizer)
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
            GetCountryByIdQuery query,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (query.Id == Guid.Empty)
            {
                return;
            }

            var countryExists = await _context.Countries
                .AnyAsync(country => country.Id == query.Id, cancellationToken);

            if (!countryExists)
            {
                context.AddFailure(
                    nameof(GetCountryByIdQuery.Id),
                    _localizer[LocalizationKeys.Countries.CountryNotFound].Value);
            }
        }
    }
}
