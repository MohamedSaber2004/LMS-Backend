using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingOption
{
    public class UpdateOnboardingOptionCommandValidator : AbstractValidator<UpdateOnboardingOptionCommand>
    {
        private const int LabelMaxLength = 200;
        private const int ValueMaxLength = 100;

        public UpdateOnboardingOptionCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.OptionNotFound].Value);

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
        }
    }
}
