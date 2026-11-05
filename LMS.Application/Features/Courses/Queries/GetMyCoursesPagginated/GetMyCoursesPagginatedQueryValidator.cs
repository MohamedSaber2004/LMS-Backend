using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetMyCoursesPagginated
{
    public class GetMyCoursesPagginatedQueryValidator
        : AbstractValidator<GetMyCoursesPagginatedQuery>
    {
        public GetMyCoursesPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Courses.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, PagginatedResult<MyCourseDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Courses.PageSizeInvalid,
                    PagginatedResult<MyCourseDto>.MaxPageSize].Value);
        }
    }
}
