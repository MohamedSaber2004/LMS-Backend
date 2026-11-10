using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateCourse
{
    public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
    {
        private const int TitleMaxLength = 200;
        private const int DescriptionMaxLength = 20000;

        private readonly IImageValidator _imageValidator;

        public UpdateCourseCommandValidator(
            IImageValidator imageValidator,
            IStringLocalizer<Messages> localizer)
        {
            _imageValidator = imageValidator;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value);

            RuleFor(command => command.Title)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .MaximumLength(TitleMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.TitleTooLong, TitleMaxLength].Value);

            RuleFor(command => command.Description)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.DescriptionRequired].Value)
                .Must(description => !string.IsNullOrWhiteSpace(description))
                .WithMessage(localizer[LocalizationKeys.Courses.DescriptionRequired].Value)
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.DescriptionTooLong, DescriptionMaxLength].Value);

            RuleFor(command => command.Level)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Courses.LevelInvalid].Value);

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Courses.OrderMustBeNonNegative].Value);

            RuleFor(command => command.Price)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Courses.PriceMustBePositive].Value);

            RuleFor(command => command.CoverUrl)
                .MaximumLength(CoursesMediaChecks.MaxUrlLength)
                .WithMessage(localizer[LocalizationKeys.Courses.UrlTooLong, CoursesMediaChecks.MaxUrlLength].Value);

            RuleFor(command => command.CoverFile)
                .Custom((file, context) => CoursesMediaChecks.ValidateCover(
                    file,
                    nameof(UpdateCourseCommand.CoverFile),
                    context,
                    _imageValidator,
                    localizer));
        }
    }
}
