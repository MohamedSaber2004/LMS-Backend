using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQueryValidator : AbstractValidator<GetTeacherRescheduleRequestsQuery>
    {
        public GetTeacherRescheduleRequestsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageSizeInvalid].Value);
        }
    }
}
