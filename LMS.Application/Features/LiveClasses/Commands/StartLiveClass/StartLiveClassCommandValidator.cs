using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.StartLiveClass
{
    public class StartLiveClassCommandValidator : AbstractValidator<StartLiveClassCommand>
    {
        public StartLiveClassCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.LiveClasses.SessionIdRequired].Value);
        }
    }
}

