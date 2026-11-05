using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetActiveCourseById
{
    public class GetActiveCourseByIdQueryValidator : AbstractValidator<GetActiveCourseByIdQuery>
    {
        public GetActiveCourseByIdQueryValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value)
                .MustAsync((courseId, cancellationToken) =>
                    CoursesValidation.CourseExistsAsync(context, courseId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.CourseNotFound].Value);
        }
    }
}
