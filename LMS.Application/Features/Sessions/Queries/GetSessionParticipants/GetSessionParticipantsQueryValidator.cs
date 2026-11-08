using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQueryValidator : AbstractValidator<GetSessionParticipantsQuery>
    {
        public GetSessionParticipantsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SessionIdRequired].Value);
        }
    }
}
