using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingOption
{
    public class AddOnboardingOptionCommandValidator : AbstractValidator<AddOnboardingOptionCommand>
    {
        private const int LabelMaxLength = 200;
        private const int ValueMaxLength = 100;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddOnboardingOptionCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Label)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.OptionLabelRequired].Value)
                .MaximumLength(LabelMaxLength)
                .WithMessage(localizer[LocalizationKeys.Onboarding.OptionLabelTooLong, LabelMaxLength].Value);

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.OptionValueRequired].Value)
                .MaximumLength(ValueMaxLength)
                .WithMessage(localizer[LocalizationKeys.Onboarding.OptionValueTooLong, ValueMaxLength].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    var question = await _context.OnboardingQuestions
                        .FirstOrDefaultAsync(item => item.Id == command.QuestionId, cancellationToken);

                    if (question is null)
                    {
                        validationContext.AddFailure(
                            nameof(command.QuestionId),
                            _localizer[LocalizationKeys.Onboarding.QuestionNotFound].Value);
                        return;
                    }

                    if (question.Type == OnboardingQuestionType.Slider)
                    {
                        validationContext.AddFailure(
                            nameof(command.QuestionId),
                            _localizer[LocalizationKeys.Onboarding.SliderNoOptionsAllowed].Value);
                        return;
                    }

                    var value = command.Value?.Trim();
                    if (!string.IsNullOrWhiteSpace(value)
                        && question.Options.Any(option => option.Value == value))
                    {
                        validationContext.AddFailure(
                            nameof(command.Value),
                            _localizer[LocalizationKeys.Onboarding.DuplicateOptionValue].Value);
                    }
                });
        }
    }
}
