using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.SubmitOnboardingAnswers
{
    public class SubmitOnboardingAnswersCommandValidator : AbstractValidator<SubmitOnboardingAnswersCommand>
    {
        private const int MaxAnswersPerRequest = 50;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public SubmitOnboardingAnswersCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Answers)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.AnswersRequired].Value);

            RuleFor(x => x.Answers)
                .Must(answers => (answers?.Count ?? 0) <= MaxAnswersPerRequest)
                .WithMessage(localizer[LocalizationKeys.Onboarding.AnswersRequired].Value);

            RuleFor(x => x.Answers)
                .Must(answers => (answers ?? new List<OnboardingAnswerInput>())
                    .Select(answer => answer.QuestionId).Distinct().Count() == (answers?.Count ?? 0))
                .WithMessage(localizer[LocalizationKeys.Onboarding.DuplicateAnswerQuestion].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    var answers = command.Answers ?? new List<OnboardingAnswerInput>();
                    var questionIds = answers.Select(answer => answer.QuestionId).ToArray();
                    if (questionIds.Length == 0)
                    {
                        return;
                    }

                    var questions = await _context.OnboardingQuestions
                        .Include(item => item.Options)
                        .Where(item => questionIds.Contains(item.Id))
                        .ToDictionaryAsync(item => item.Id, cancellationToken);

                    foreach (var answer in answers)
                    {
                        if (!questions.TryGetValue(answer.QuestionId, out var question))
                        {
                            validationContext.AddFailure(
                                nameof(OnboardingAnswerInput.QuestionId),
                                _localizer[LocalizationKeys.Onboarding.QuestionNotFound].Value);
                            continue;
                        }

                        if (!question.IsActive)
                        {
                            validationContext.AddFailure(
                                nameof(OnboardingAnswerInput.QuestionId),
                                _localizer[LocalizationKeys.Onboarding.QuestionNotActive].Value);
                            continue;
                        }

                        switch (question.Type)
                        {
                            case OnboardingQuestionType.Slider:
                                ValidateSliderAnswer(question, answer, validationContext);
                                break;
                            case OnboardingQuestionType.MultiSelect:
                                ValidateSelectAnswer(question, answer, validationContext, requireExactlyOne: false);
                                break;
                            default:
                                ValidateSelectAnswer(question, answer, validationContext, requireExactlyOne: true);
                                break;
                        }
                    }
                });
        }

        private void ValidateSliderAnswer(
            Domain.Entities.OnboardingQuestion question,
            OnboardingAnswerInput answer,
            CustomContext validationContext)
        {
            if (answer.NumericValue is null)
            {
                validationContext.AddFailure(
                    nameof(OnboardingAnswerInput.NumericValue),
                    _localizer[LocalizationKeys.Onboarding.NumericValueRequired].Value);
                return;
            }

            var min = question.MinValue ?? 0;
            var max = question.MaxValue ?? 0;

            if (answer.NumericValue < min || answer.NumericValue > max)
            {
                validationContext.AddFailure(
                    nameof(OnboardingAnswerInput.NumericValue),
                    _localizer[LocalizationKeys.Onboarding.NumericValueOutOfRange, min, max].Value);
            }
        }

        private void ValidateSelectAnswer(
            Domain.Entities.OnboardingQuestion question,
            OnboardingAnswerInput answer,
            CustomContext validationContext,
            bool requireExactlyOne)
        {
            var optionIds = answer.OptionIds ?? new List<Guid>();

            if (optionIds.Count == 0)
            {
                validationContext.AddFailure(
                    nameof(OnboardingAnswerInput.OptionIds),
                    _localizer[LocalizationKeys.Onboarding.AtLeastOneOptionRequired].Value);
                return;
            }

            if (requireExactlyOne && optionIds.Count != 1)
            {
                validationContext.AddFailure(
                    nameof(OnboardingAnswerInput.OptionIds),
                    _localizer[LocalizationKeys.Onboarding.SingleOptionRequired].Value);
                return;
            }

            var validOptionIds = question.Options
                .Where(option => option.IsActive)
                .Select(option => option.Id)
                .ToHashSet();

            if (!optionIds.All(validOptionIds.Contains))
            {
                validationContext.AddFailure(
                    nameof(OnboardingAnswerInput.OptionIds),
                    _localizer[LocalizationKeys.Onboarding.AnswerOptionNotFromQuestion].Value);
            }
        }
    }
}
