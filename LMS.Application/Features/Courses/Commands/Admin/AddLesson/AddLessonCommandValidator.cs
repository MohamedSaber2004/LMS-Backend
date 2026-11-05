using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AddLesson
{
    public class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
    {
        private const int TitleMaxLength = 200;

        private readonly ILmsDbContext _context;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddLessonCommandValidator(
            ILmsDbContext context,
            IVideoValidator videoValidator,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
            _localizer = localizer;

            RuleFor(command => command.CourseId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value)
                .MustAsync((courseId, cancellationToken) =>
                    CoursesValidation.CourseExistsAsync(_context, courseId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.CourseNotFound].Value);

            RuleFor(command => command.Title)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage(localizer[LocalizationKeys.Courses.TitleRequired].Value)
                .MaximumLength(TitleMaxLength)
                .WithMessage(localizer[LocalizationKeys.Courses.TitleTooLong, TitleMaxLength].Value);

            RuleFor(command => command)
                .Must(command => command.VideoFile is not null || !string.IsNullOrWhiteSpace(command.VideoUrl))
                .WithMessage(localizer[LocalizationKeys.Courses.VideoSourceRequired].Value);

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
                    nameof(AddLessonCommand.VideoFile),
                    context,
                    _videoValidator,
                    _localizer));

            RuleFor(command => command.StudyBookFile)
                .Custom((file, context) => CoursesMediaChecks.ValidateStudyBook(
                    file,
                    nameof(AddLessonCommand.StudyBookFile),
                    context,
                    _fileValidator,
                    _localizer));
        }
    }
}
