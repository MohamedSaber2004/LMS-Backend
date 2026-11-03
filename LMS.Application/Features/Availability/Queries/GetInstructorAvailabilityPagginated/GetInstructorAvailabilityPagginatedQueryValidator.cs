using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityPagginated
{
    public class GetInstructorAvailabilityPagginatedQueryValidator
        : AbstractValidator<GetInstructorAvailabilityPagginatedQuery>
    {
        public GetInstructorAvailabilityPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.Day)
                .Must(day => day is null || Enum.IsDefined(day.Value))
                .WithMessage(localizer[LocalizationKeys.Availability.DayInvalid].Value);

            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Availability.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, PagginatedResult<InstructorAvailabilityRowDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Availability.PageSizeInvalid,
                    PagginatedResult<InstructorAvailabilityRowDto>.MaxPageSize].Value);
        }
    }
}
