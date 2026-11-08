using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.LeaveLiveClass
{
    public class LeaveLiveClassCommandValidator : AbstractValidator<LeaveLiveClassCommand>
    {
        public LeaveLiveClassCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.LiveClasses.SessionIdRequired].Value);
        }
    }
}
