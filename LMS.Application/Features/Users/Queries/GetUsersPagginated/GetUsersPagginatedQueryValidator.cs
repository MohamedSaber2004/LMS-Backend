using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Queries.GetUsersPagginated
{
    public class GetUsersPagginatedQueryValidator : AbstractValidator<GetUsersPagginatedQuery>
    {
        private const int SearchMaxLength = 100;

        public GetUsersPagginatedQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Users.PageNumberInvalid].Value);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagginatedResult<UserDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Users.PageSizeInvalid,
                    PagginatedResult<UserDto>.MaxPageSize].Value);

            RuleFor(x => x.Role)
                .Must(role => !role.HasValue || Enum.IsDefined(typeof(UserType), role.Value))
                .WithMessage(localizer[LocalizationKeys.Users.InvalidRole].Value);

            RuleFor(x => x.Search)
                .MaximumLength(SearchMaxLength)
                .WithMessage(localizer[LocalizationKeys.Users.SearchTooLong, SearchMaxLength].Value)
                .When(x => !string.IsNullOrWhiteSpace(x.Search));
        }
    }
}
