using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.LiveClasses.SessionIdRequired].Value);
        }
    }
}
