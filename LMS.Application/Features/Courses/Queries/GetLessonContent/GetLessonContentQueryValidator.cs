using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetLessonContent
{
    public class GetLessonContentQueryValidator : AbstractValidator<GetLessonContentQuery>
    {
        public GetLessonContentQueryValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.LessonId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value)
                .MustAsync((lessonId, cancellationToken) =>
                    CoursesValidation.LessonExistsAsync(context, lessonId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.LessonNotFound].Value);
        }
    }
}
