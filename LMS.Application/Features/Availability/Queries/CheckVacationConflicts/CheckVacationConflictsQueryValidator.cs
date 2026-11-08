using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQueryValidator : AbstractValidator<CheckVacationConflictsQuery>
    {
        public CheckVacationConflictsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query)
                .Must(q => !q.FromUtc.HasValue || !q.ToUtc.HasValue || q.FromUtc <= q.ToUtc)
                .WithMessage(localizer[LocalizationKeys.Availability.DateRangeInvalid].Value);
        }
    }
}
