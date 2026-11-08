using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.JoinLiveClass
{
    public class JoinLiveClassCommandValidator : AbstractValidator<JoinLiveClassCommand>
    {
        public JoinLiveClassCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.LiveClasses.SessionIdRequired].Value);
        }
    }
}
