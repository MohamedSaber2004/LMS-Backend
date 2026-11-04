using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetMyBookings
{
    public class GetMyBookingsQueryValidator : AbstractValidator<GetMyBookingsQuery>
    {
        public GetMyBookingsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.Tab)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Sessions.TabInvalid].Value);

            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(PagginatedResult<SessionDto>.MaxPageSize)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageSizeInvalid, PagginatedResult<SessionDto>.MaxPageSize].Value);
        }
    }
}
