using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.EnrollCourse
{
    public class EnrollCourseCommandValidator : AbstractValidator<EnrollCourseCommand>
    {
        public EnrollCourseCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.CourseId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value)
                .MustAsync((courseId, cancellationToken) =>
                    CoursesValidation.CourseExistsAsync(context, courseId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.CourseNotFound].Value);
        }
    }
}
