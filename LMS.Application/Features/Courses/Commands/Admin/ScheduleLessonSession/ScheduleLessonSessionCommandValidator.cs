using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.ScheduleLessonSession
{
    public class ScheduleLessonSessionCommandValidator : AbstractValidator<ScheduleLessonSessionCommand>
    {
        public ScheduleLessonSessionCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.LessonId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value);

            RuleFor(x => x.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.InstructorIdRequired].Value);

            RuleFor(x => x.StartsAtUtc)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonStartsAtRequired].Value);

            RuleFor(x => x.EndsAtUtc)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonEndsAtRequired].Value)
                .GreaterThan(x => x.StartsAtUtc)
                .WithMessage(localizer[LocalizationKeys.Courses.LessonTimeRangeInvalid].Value);

            RuleFor(x => x.SeatCapacity)
                .GreaterThan(0);
        }
    }
}

