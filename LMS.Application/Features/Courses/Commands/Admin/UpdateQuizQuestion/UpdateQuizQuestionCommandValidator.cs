using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateQuizQuestion
{
    public class UpdateQuizQuestionCommandValidator : AbstractValidator<UpdateQuizQuestionCommand>
    {
        private const int TextMaxLength = 500;
        private const int OptionTextMaxLength = 300;
        private const int MinOptions = 2;
        private const int MaxOptions = 10;

        private readonly ILmsDbContext _context;

        public UpdateQuizQuestionCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionIdRequired].Value)
                .MustAsync((questionId, cancellationToken) =>
                    CoursesValidation.QuestionExistsAsync(_context, questionId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionNotFound].Value);

            RuleFor(command => command.Text)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionTextRequired].Value)
                .Must(text => !string.IsNullOrWhiteSpace(text))
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionTextRequired].Value)
                .MaximumLength(TextMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionTooLong, TextMaxLength].Value);

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Courses.OrderMustBeNonNegative].Value);

            RuleFor(command => command.Options)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.OptionsRequired].Value)
                .Must(options => options is { Count: >= MinOptions and <= MaxOptions })
                .WithMessage(localizer[
                    LocalizationKeys.Courses.OptionsCountInvalid,
                    MinOptions,
                    MaxOptions].Value);

            RuleForEach(command => command.Options).ChildRules(option =>
            {
                option.RuleFor(item => item.Text)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Courses.OptionTextRequired].Value)
                    .Must(text => !string.IsNullOrWhiteSpace(text))
                    .WithMessage(localizer[LocalizationKeys.Courses.OptionTextRequired].Value)
                    .MaximumLength(OptionTextMaxLength)
                    .WithMessage(
                        localizer[LocalizationKeys.Courses.OptionTextTooLong, OptionTextMaxLength].Value);
            });

            RuleFor(command => command.Options)
                .Must(options => options!.Count(item => item.IsCorrect) == 1)
                .WithMessage(localizer[LocalizationKeys.Courses.ExactlyOneCorrectOptionRequired].Value)
                .When(command => command.Options is { Count: > 0 });
        }
    }
}
