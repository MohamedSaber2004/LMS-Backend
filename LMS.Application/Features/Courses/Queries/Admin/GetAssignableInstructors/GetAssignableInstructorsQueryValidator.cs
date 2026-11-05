using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.Admin.GetAssignableInstructors
{
    public class GetAssignableInstructorsQueryValidator : AbstractValidator<GetAssignableInstructorsQuery>
    {
        private const int SearchMaxLength = 100;

        public GetAssignableInstructorsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Courses.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, PagginatedResult<InstructorRefDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Courses.PageSizeInvalid,
                    PagginatedResult<InstructorRefDto>.MaxPageSize].Value);

            RuleFor(query => query.Search)
                .MaximumLength(SearchMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.SearchTooLong, SearchMaxLength].Value)
                .When(query => !string.IsNullOrWhiteSpace(query.Search));
        }
    }
}
