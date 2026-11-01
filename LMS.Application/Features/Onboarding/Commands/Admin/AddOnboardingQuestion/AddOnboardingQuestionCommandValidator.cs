using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingQuestion
{
    public class AddOnboardingQuestionCommandValidator : AbstractValidator<AddOnboardingQuestionCommand>
    {
        private const int KeyMaxLength = 50;
        private const int TitleMaxLength = 200;
        private const int OptionLabelMaxLength = 200;
        private const int OptionValueMaxLength = 100;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddOnboardingQuestionCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Key)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.QuestionKeyRequired].Value)
                .MaximumLength(KeyMaxLength)
                .WithMessage(localizer[LocalizationKeys.Onboarding.QuestionKeyTooLong, KeyMaxLength].Value)
                .Matches("^[a-z0-9_]+$")
                .WithMessage(localizer[LocalizationKeys.Onboarding.QuestionKeyInvalidFormat].Value);

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.TitleRequired].Value)
                .MaximumLength(TitleMaxLength)
                .WithMessage(localizer[LocalizationKeys.Onboarding.TitleTooLong, TitleMaxLength].Value);

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Onboarding.InvalidQuestionType].Value);

            When(x => x.Type == OnboardingQuestionType.Slider, () =>
            {
                RuleFor(x => x.MinValue)
                    .NotNull()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.SliderBoundsRequired].Value);

                RuleFor(x => x.MaxValue)
                    .NotNull()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.SliderBoundsRequired].Value);

                RuleFor(x => x.Step)
                    .NotNull()
                    .GreaterThan(0)
                    .WithMessage(localizer[LocalizationKeys.Onboarding.InvalidSliderBounds].Value);

                RuleFor(x => x)
                    .Must(command => command.MinValue is null
                        || command.MaxValue is null
                        || command.MaxValue > command.MinValue)
                    .WithMessage(localizer[LocalizationKeys.Onboarding.InvalidSliderBounds].Value);

                RuleFor(x => x.Options)
                    .Empty()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.SliderNoOptionsAllowed].Value);
            });

            When(x => x.Type != OnboardingQuestionType.Slider, () =>
            {
                RuleFor(x => x.Options)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.OptionsRequired].Value);
            });

            RuleForEach(x => x.Options).ChildRules(option =>
            {
                option.RuleFor(x => x.Label)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.OptionLabelRequired].Value)
                    .MaximumLength(OptionLabelMaxLength)
                    .WithMessage(localizer[LocalizationKeys.Onboarding.OptionLabelTooLong, OptionLabelMaxLength].Value);

                option.RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithMessage(localizer[LocalizationKeys.Onboarding.OptionValueRequired].Value)
                    .MaximumLength(OptionValueMaxLength)
                    .WithMessage(localizer[LocalizationKeys.Onboarding.OptionValueTooLong, OptionValueMaxLength].Value);
            });

            RuleFor(x => x.Options)
                .Must(options => (options ?? new List<OnboardingOptionInput>())
                    .Select(option => option.Value?.Trim()).Distinct().Count() == options!.Count)
                .WithMessage(localizer[LocalizationKeys.Onboarding.DuplicateOptionValue].Value)
                .When(x => (x.Options?.Count ?? 0) > 0);

            RuleFor(x => x.Key)
                .MustAsync(BeUniqueKeyAsync)
                .WithMessage(localizer[LocalizationKeys.Onboarding.QuestionKeyAlreadyExists].Value);
        }

        private async Task<bool> BeUniqueKeyAsync(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return true;
            }

            var normalizedKey = key.Trim();
            return !await _context.OnboardingQuestions
                .AnyAsync(question => question.Key == normalizedKey, cancellationToken);
        }
    }
}
