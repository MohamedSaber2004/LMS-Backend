using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.UpdateLessonProgress
{
    public class UpdateLessonProgressCommandValidator : AbstractValidator<UpdateLessonProgressCommand>
    {
        public UpdateLessonProgressCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.LessonId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value)
                .MustAsync((lessonId, cancellationToken) =>
                    CoursesValidation.LessonExistsAsync(context, lessonId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.LessonNotFound].Value);

            RuleFor(command => command.LastWatchedSeconds)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Courses.WatchSecondsMustBeNonNegative].Value);
        }
    }
}
