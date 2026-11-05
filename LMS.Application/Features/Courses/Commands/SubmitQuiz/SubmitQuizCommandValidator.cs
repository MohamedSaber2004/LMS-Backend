using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.SubmitQuiz
{
    public class SubmitQuizCommandValidator : AbstractValidator<SubmitQuizCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public SubmitQuizCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(command => command.LessonId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value)
                .MustAsync((lessonId, cancellationToken) =>
                    CoursesValidation.LessonExistsAsync(_context, lessonId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.LessonNotFound].Value);

            RuleFor(command => command.Answers)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.AnswersRequired].Value);

            RuleFor(command => command.Answers)
                .Must(answers => answers is { Count: > 0 }
                    && answers.Select(answer => answer.QuestionId).Distinct().Count() == answers.Count)
                .WithMessage(localizer[LocalizationKeys.Courses.DuplicateAnswerNotAllowed].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateReferencesAsync);
        }

        private async Task ValidateReferencesAsync(
            SubmitQuizCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.LessonId == Guid.Empty || command.Answers is not { Count: > 0 })
            {
                return;
            }

            var lessonExists = await CoursesValidation.LessonExistsAsync(
                _context, command.LessonId, cancellationToken);

            if (!lessonExists)
            {
                return;
            }

            var questionIds = command.Answers.Select(answer => answer.QuestionId).ToArray();

            var lessonQuestionIds = await _context.QuizQuestions
                .Where(question => question.LessonId == command.LessonId
                    && question.IsActive
                    && questionIds.Contains(question.Id))
                .Select(question => question.Id)
                .ToListAsync(cancellationToken);

            if (questionIds.Any(questionId => !lessonQuestionIds.Contains(questionId)))
            {
                context.AddFailure(
                    nameof(SubmitQuizCommand.Answers),
                    _localizer[LocalizationKeys.Courses.AnswerQuestionInvalid].Value);

                return;
            }

            var optionIds = command.Answers.Select(answer => answer.SelectedOptionId).ToArray();

            var validOptions = await _context.QuizOptions
                .Where(option => option.IsActive
                    && optionIds.Contains(option.Id)
                    && questionIds.Contains(option.QuizQuestionId))
                .Select(option => new { option.Id, option.QuizQuestionId })
                .ToListAsync(cancellationToken);

            var hasInvalidOption = command.Answers.Any(answer => !validOptions.Any(option =>
                option.Id == answer.SelectedOptionId && option.QuizQuestionId == answer.QuestionId));

            if (hasInvalidOption)
            {
                context.AddFailure(
                    nameof(SubmitQuizCommand.Answers),
                    _localizer[LocalizationKeys.Courses.AnswerOptionInvalid].Value);
            }
        }
    }
}
