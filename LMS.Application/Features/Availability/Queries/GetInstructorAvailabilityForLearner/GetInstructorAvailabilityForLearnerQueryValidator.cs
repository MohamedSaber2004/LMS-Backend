using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityForLearner
{
    public class GetInstructorAvailabilityForLearnerQueryValidator
        : AbstractValidator<GetInstructorAvailabilityForLearnerQuery>
    {
        public GetInstructorAvailabilityForLearnerQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorIdRequired].Value);
        }
    }
}
