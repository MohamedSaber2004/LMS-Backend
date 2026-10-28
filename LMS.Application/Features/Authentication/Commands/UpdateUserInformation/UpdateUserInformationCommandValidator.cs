using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.UpdateUserInformation
{
    public class UpdateUserInformationCommandValidator : AbstractValidator<UpdateUserInformationCommand>
    {
        private const int FullNameMaxLength = 200;
        private const int ProfilePictureUrlMaxLength = 500;

        public UpdateUserInformationCommandValidator(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameRequired].Value)
                .MaximumLength(FullNameMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameTooLong, FullNameMaxLength].Value);

            RuleFor(x => x.Language)
                .Must(language => !language.HasValue || Enum.IsDefined(typeof(Language), language.Value))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidLanguage].Value);

            RuleFor(x => x.ProfilePictureUrl)
                .MaximumLength(ProfilePictureUrlMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.ProfilePictureUrlTooLong, ProfilePictureUrlMaxLength].Value)
                .When(x => !string.IsNullOrWhiteSpace(x.ProfilePictureUrl));

            RuleFor(x => x.CountryId)
                .CustomAsync(async (countryId, validationContext, cancellationToken) =>
                {
                    if (countryId is null)
                        return;

                    var exists = await context.Countries.AnyAsync(country => country.Id == countryId.Value, cancellationToken);
                    if (!exists)
                    {
                        validationContext.AddFailure(localizer[LocalizationKeys.Authentication.CountryNotFound].Value);
                    }
                });

            RuleFor(x => x)
                .CustomAsync(async (_, _, _) =>
                {
                    if (!currentUserService.IsAuthenticated || currentUserService.UserId == Guid.Empty)
                    {
                        throw new UnAuthorizedException();
                    }

                    var user = await userManager.FindByIdAsync(currentUserService.UserId.ToString());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }
                });
        }
    }
}
