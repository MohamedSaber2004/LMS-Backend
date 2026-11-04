using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetMyAvailabilityCalendar
{
    public class GetMyAvailabilityCalendarQueryValidator : AbstractValidator<GetMyAvailabilityCalendarQuery>
    {
        public const int MaxRangeDays = 92;

        public GetMyAvailabilityCalendarQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query)
                .Must(query => query.From != default && query.To != default)
                .WithMessage(localizer[LocalizationKeys.Availability.DateRangeRequired].Value);

            RuleFor(query => query)
                .Must(query => query.To >= query.From)
                .WithMessage(localizer[LocalizationKeys.Availability.DateRangeInvalid].Value)
                .When(query => query.From != default && query.To != default);

            RuleFor(query => query)
                .Must(query => query.To.DayNumber - query.From.DayNumber + 1 <= MaxRangeDays)
                .WithMessage(localizer[LocalizationKeys.Availability.DateRangeTooLarge, MaxRangeDays].Value)
                .When(query => query.From != default && query.To != default);
        }
    }
}
