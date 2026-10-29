using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Queries.GetAllCountriesPagginated
{
    public class GetAllCountriesPagginatedQueryValidator : AbstractValidator<GetAllCountriesPagginatedQuery>
    {
        public GetAllCountriesPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Countries.PageNumberInvalid].Value);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagginatedResult<CountryDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Countries.PageSizeInvalid,
                    PagginatedResult<CountryDto>.MaxPageSize].Value);
        }
    }
}
