using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetBookableSlots
{
    public class GetBookableSlotsQueryValidator : AbstractValidator<GetBookableSlotsQuery>
    {
        private const int MaxRangeDays = 92;

        public GetBookableSlotsQueryValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.InstructorIdRequired].Value);

            RuleFor(query => query)
                .Must(query => query.From != default && query.To != default)
                .WithMessage(localizer[LocalizationKeys.Sessions.DateRangeRequired].Value);

            RuleFor(query => query.To)
                .GreaterThanOrEqualTo(query => query.From)
                .WithMessage(localizer[LocalizationKeys.Sessions.DateRangeInvalid].Value);

            RuleFor(query => query)
                .Must(query => (query.To.DayNumber - query.From.DayNumber) + 1 <= MaxRangeDays)
                .WithMessage(localizer[LocalizationKeys.Sessions.DateRangeTooLarge, MaxRangeDays].Value);

            RuleFor(query => query.InstructorId)
                .MustAsync((instructorId, cancellationToken) => SessionsValidation.InstructorIsBookableAsync(
                    context, instructorId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Sessions.InstructorNotBookable].Value);
        }
    }
}
