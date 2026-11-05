using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteLesson
{
    public class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteLessonCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeleteLessonCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var lessonExists = await _context.Lessons
                .AnyAsync(lesson => lesson.Id == command.Id, cancellationToken);

            if (!lessonExists)
            {
                return;
            }

            var hasQuestions = await _context.QuizQuestions
                .AnyAsync(question => question.LessonId == command.Id, cancellationToken);

            if (hasQuestions)
            {
                context.AddFailure(
                    nameof(DeleteLessonCommand.Id),
                    _localizer[LocalizationKeys.Courses.LessonHasQuizQuestions].Value);
            }
        }
    }
}
