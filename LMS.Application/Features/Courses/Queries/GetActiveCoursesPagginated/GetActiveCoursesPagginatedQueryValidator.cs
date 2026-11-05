using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetActiveCoursesPagginated
{
    public class GetActiveCoursesPagginatedQueryValidator
        : AbstractValidator<GetActiveCoursesPagginatedQuery>
    {
        private const int SearchMaxLength = 100;

        public GetActiveCoursesPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Courses.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, PagginatedResult<CourseListItemDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Courses.PageSizeInvalid,
                    PagginatedResult<CourseListItemDto>.MaxPageSize].Value);

            RuleFor(query => query.Search)
                .MaximumLength(SearchMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.SearchTooLong, SearchMaxLength].Value)
                .When(query => !string.IsNullOrWhiteSpace(query.Search));

            RuleFor(query => query.Level)
                .Must(level => level is null || Enum.IsDefined(level.Value))
                .WithMessage(localizer[LocalizationKeys.Courses.LevelInvalid].Value);
        }
    }
}
