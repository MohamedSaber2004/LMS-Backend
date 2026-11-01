using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingQuestion
{
    public class UpdateOnboardingQuestionCommandValidator : AbstractValidator<UpdateOnboardingQuestionCommand>
    {
        private const int TitleMaxLength = 200;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateOnboardingQuestionCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.QuestionNotFound].Value);

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Onboarding.TitleRequired].Value)
                .MaximumLength(TitleMaxLength)
                .WithMessage(localizer[LocalizationKeys.Onboarding.TitleTooLong, TitleMaxLength].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.Id == Guid.Empty)
                    {
                        return;
                    }

                    var question = await _context.OnboardingQuestions
                        .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken);

                    if (question is null)
                    {
                        validationContext.AddFailure(
                            nameof(command.Id),
                            _localizer[LocalizationKeys.Onboarding.QuestionNotFound].Value);
                        return;
                    }

                    if (question.Type != OnboardingQuestionType.Slider)
                    {
                        return;
                    }

                    var isSliderValid = command.MinValue is not null
                        && command.MaxValue is not null
                        && command.Step is not null
                        && command.Step > 0
                        && command.MaxValue > command.MinValue;

                    if (!isSliderValid)
                    {
                        validationContext.AddFailure(
                            nameof(command.MaxValue),
                            _localizer[LocalizationKeys.Onboarding.InvalidSliderBounds].Value);
                    }
                });
        }
    }
}
