using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Commands.UpdateInstructorProfile
{
    public class UpdateInstructorProfileCommandValidator : AbstractValidator<UpdateInstructorProfileCommand>
    {
        private const int SpecializationMaxLength = 150;
        private const int BioMinLength = 30;
        private const int BioMaxLength = 2000;
        private const int EducationMaxLength = 500;
        private const int LinkedInUrlMaxLength = 500;
        private const int MinYearsOfExperience = 0;
        private const int MaxYearsOfExperience = 60;
        private const decimal MaxHourlyRate = 10000m;
        private const int FullNameMaxLength = 200;
        private const int ProfilePictureUrlMaxLength = 500;

        public UpdateInstructorProfileCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Specialization)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.SpecializationRequired].Value)
                .MaximumLength(SpecializationMaxLength)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.SpecializationTooLong,
                    SpecializationMaxLength].Value);

            RuleFor(x => x.Bio)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.BioRequired].Value)
                .MinimumLength(BioMinLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.BioTooShort, BioMinLength].Value)
                .MaximumLength(BioMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.BioTooLong, BioMaxLength].Value);

            RuleFor(x => x.YearsOfExperience)
                .NotNull()
                .WithMessage(localizer[LocalizationKeys.Authentication.YearsOfExperienceRequired].Value)
                .Must(value => value.HasValue
                    && value.Value >= MinYearsOfExperience
                    && value.Value <= MaxYearsOfExperience)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.InvalidYearsOfExperience,
                    MinYearsOfExperience,
                    MaxYearsOfExperience].Value);

            RuleFor(x => x.Education)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.EducationRequired].Value)
                .MaximumLength(EducationMaxLength)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.EducationTooLong,
                    EducationMaxLength].Value);

            RuleFor(x => x.HourlyRate)
                .Must(rate => rate is null || rate.Value <= MaxHourlyRate)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.HourlyRateTooLarge,
                    MaxHourlyRate].Value);

            RuleFor(x => x.FullName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage(localizer[LocalizationKeys.Authentication.FullNameRequired].Value)
                .MaximumLength(FullNameMaxLength)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.FullNameTooLong,
                    FullNameMaxLength].Value)
                .When(x => x.FullName is not null);

            RuleFor(x => x.Language)
                .Must(language => !language.HasValue || Enum.IsDefined(typeof(Language), language.Value))
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidLanguage].Value);

            RuleFor(x => x.ProfilePictureUrl)
                .MaximumLength(ProfilePictureUrlMaxLength)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.ProfilePictureUrlTooLong,
                    ProfilePictureUrlMaxLength].Value)
                .When(x => !string.IsNullOrWhiteSpace(x.ProfilePictureUrl));

            RuleFor(x => x.CountryId)
                .CustomAsync(async (countryId, validationContext, cancellationToken) =>
                {
                    if (countryId is null)
                    {
                        return;
                    }

                    var exists = await context.Countries
                        .AnyAsync(country => country.Id == countryId.Value, cancellationToken);

                    if (!exists)
                    {
                        validationContext.AddFailure(
                            localizer[LocalizationKeys.Authentication.CountryNotFound].Value);
                    }
                });

            RuleFor(x => x.LinkedInUrl)
                .MaximumLength(LinkedInUrlMaxLength)
                .WithMessage(localizer[
                    LocalizationKeys.Authentication.LinkedInUrlTooLong,
                    LinkedInUrlMaxLength].Value)
                .Must(BeValidUrl)
                .WithMessage(localizer[LocalizationKeys.Authentication.InvalidLinkedInUrl].Value)
                .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));

            RuleFor(x => x)
                .CustomAsync(async (_, validationContext, cancellationToken) =>
                {
                    var userId = currentUserService.UserId;
                    if (userId == Guid.Empty)
                    {
                        return;
                    }

                    var status = await context.Instructors
                        .AsNoTracking()
                        .Where(instructor => instructor.UserId == userId)
                        .Select(instructor => (InstructorApplicationStatus?)instructor.Application!.Status)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (status is null)
                    {
                        validationContext.AddFailure(
                            localizer[LocalizationKeys.Authentication.UserNotFound].Value);
                        return;
                    }

                    if (status != InstructorApplicationStatus.Approved)
                    {
                        validationContext.AddFailure(
                            localizer[LocalizationKeys.InstructorProfile.EditNotAllowedBeforeApproval].Value);
                    }
                });
        }

        private static bool BeValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
