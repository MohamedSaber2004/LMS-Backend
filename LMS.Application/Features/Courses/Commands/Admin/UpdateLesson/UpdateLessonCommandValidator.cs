using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateLesson
{
    public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
    {
        private const int TitleMaxLength = 200;

        private readonly ILmsDbContext _context;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateLessonCommandValidator(
            ILmsDbContext context,
            IVideoValidator videoValidator,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.LessonIdRequired].Value)
                .MustAsync((lessonId, cancellationToken) =>
                    CoursesValidation.LessonExistsAsync(_context, lessonId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.LessonNotFound].Value);

            RuleFor(command => command.Title)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .MaximumLength(TitleMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.TitleTooLong, TitleMaxLength].Value);

            RuleFor(command => command.VideoUrl)
                .MaximumLength(CoursesMediaChecks.MaxUrlLength)
                .WithMessage(localizer[LocalizationKeys.Courses.UrlTooLong, CoursesMediaChecks.MaxUrlLength].Value);

            RuleFor(command => command.StudyBookUrl)
                .MaximumLength(CoursesMediaChecks.MaxUrlLength)
                .WithMessage(localizer[LocalizationKeys.Courses.UrlTooLong, CoursesMediaChecks.MaxUrlLength].Value);

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Courses.OrderMustBeNonNegative].Value);

            RuleFor(command => command.VideoFile)
                .Custom((file, context) => CoursesMediaChecks.ValidateVideo(
                    file,
                    nameof(UpdateLessonCommand.VideoFile),
                    context,
                    _videoValidator,
                    _localizer));

            RuleFor(command => command.StudyBookFile)
                .Custom((file, context) => CoursesMediaChecks.ValidateStudyBook(
                    file,
                    nameof(UpdateLessonCommand.StudyBookFile),
                    context,
                    _fileValidator,
                    _localizer));
        }
    }
}
