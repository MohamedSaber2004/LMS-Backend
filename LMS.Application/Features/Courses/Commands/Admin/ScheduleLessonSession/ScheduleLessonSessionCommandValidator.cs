using System.Globalization;
using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.ScheduleLessonSession
{
    public class ScheduleLessonSessionCommandValidator : AbstractValidator<ScheduleLessonSessionCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public ScheduleLessonSessionCommandValidator(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;

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

            RuleFor(command => command)
                .Custom(RejectClosedWindow);
        }

        private void RejectClosedWindow(
            ScheduleLessonSessionCommand command,
            CustomContext context)
        {
            if (command.EndsAtUtc == default || command.EndsAtUtc > DateTime.UtcNow)
            {
                return;
            }

            context.AddFailure(
                nameof(ScheduleLessonSessionCommand.EndsAtUtc),
                string.Format(
                    _localizer[LocalizationKeys.Courses.LessonSessionWindowInPast].Value,
                    command.StartsAtUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    command.EndsAtUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
        }
    }
}
