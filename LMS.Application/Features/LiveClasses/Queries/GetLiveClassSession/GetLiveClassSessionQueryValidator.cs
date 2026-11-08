using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Queries.GetLiveClassSession
{
    public class GetLiveClassSessionQueryValidator : AbstractValidator<GetLiveClassSessionQuery>
    {
        public GetLiveClassSessionQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.LiveClasses.SessionIdRequired].Value);
        }
    }
}

