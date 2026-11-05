using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteCourse
{
    public class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteCourseCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeleteCourseCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var courseExists = await _context.Courses
                .AnyAsync(course => course.Id == command.Id, cancellationToken);

            if (!courseExists)
            {
                return;
            }

            var hasLessons = await _context.Lessons
                .AnyAsync(lesson => lesson.CourseId == command.Id, cancellationToken);

            if (hasLessons)
            {
                context.AddFailure(
                    nameof(DeleteCourseCommand.Id),
                    _localizer[LocalizationKeys.Courses.CourseHasLessons].Value);
            }
        }
    }
}
