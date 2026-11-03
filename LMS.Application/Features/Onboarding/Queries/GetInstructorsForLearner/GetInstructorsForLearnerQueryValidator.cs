using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Queries.GetInstructorsForLearner
{
    public class GetInstructorsForLearnerQueryValidator : AbstractValidator<GetInstructorsForLearnerQuery>
    {
        public GetInstructorsForLearnerQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.MinPrice)
                .NotNull()
                .WithMessage(localizer[LocalizationKeys.Onboarding.PriceRangeInvalid].Value)
                .GreaterThanOrEqualTo(0m)
                .WithMessage(localizer[LocalizationKeys.Onboarding.PriceRangeInvalid].Value)
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .NotNull()
                .WithMessage(localizer[LocalizationKeys.Onboarding.PriceRangeInvalid].Value)
                .GreaterThan(0m)
                .WithMessage(localizer[LocalizationKeys.Onboarding.PriceRangeInvalid].Value)
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Onboarding.PageNumberInvalid].Value);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagginatedResult<InstructorCardDto>.MaxPageSize)
                .WithMessage(localizer[LocalizationKeys.Onboarding.PageSizeInvalid, PagginatedResult<InstructorCardDto>.MaxPageSize].Value);

            RuleFor(x => x.Specialization)
                .MaximumLength(150)
                .WithMessage(localizer[LocalizationKeys.Onboarding.SpecializationTooLong, 150].Value);

            RuleFor(x => x.Day)
                .Must(day => day is null || Enum.IsDefined(day.Value))
                .WithMessage(localizer[LocalizationKeys.Availability.DayInvalid].Value);

            RuleFor(x => x)
                .Must(x => x.Day.HasValue || (x.After is null && x.Before is null))
                .WithMessage(localizer[LocalizationKeys.Availability.DayRequiredForTimeFilter].Value);

            RuleFor(x => x.Before)
                .Must((x, before) => before is null || x.After is null || before > x.After)
                .WithMessage(localizer[LocalizationKeys.Availability.FilterTimeRangeInvalid].Value);
        }
    }
}
