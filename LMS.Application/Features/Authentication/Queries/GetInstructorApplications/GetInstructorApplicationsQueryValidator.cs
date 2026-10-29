using FluentValidation;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Queries.GetInstructorApplications
{
    public class GetInstructorApplicationsQueryValidator : AbstractValidator<GetInstructorApplicationsQuery>
    {
        public GetInstructorApplicationsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Status)
                .Must(status => !status.HasValue || Enum.IsDefined(typeof(InstructorApplicationStatus), status.Value))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidApplicationStatus].Value);

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Authentication.PageNumberInvalid].Value);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagginatedResult<InstructorApplicationDto>.MaxPageSize)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.PageSizeInvalid,
                    PagginatedResult<InstructorApplicationDto>.MaxPageSize].Value);
        }
    }
}
