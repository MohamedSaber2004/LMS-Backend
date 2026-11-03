using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Queries.GetHelpArticlesPagginated
{
    public class GetHelpArticlesPagginatedQueryValidator
        : AbstractValidator<GetHelpArticlesPagginatedQuery>
    {
        private const int SearchMaxLength = 100;

        public GetHelpArticlesPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, PagginatedResult<HelpArticleDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.HelpCenter.PageSizeInvalid,
                    PagginatedResult<HelpArticleDto>.MaxPageSize].Value);

            RuleFor(query => query.Search)
                .MaximumLength(SearchMaxLength)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.SearchTooLong, SearchMaxLength].Value)
                .When(query => !string.IsNullOrWhiteSpace(query.Search));
        }
    }
}
