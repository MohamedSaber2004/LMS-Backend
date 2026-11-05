using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AssignCourseInstructors
{
    public class AssignCourseInstructorsCommandValidator : AbstractValidator<AssignCourseInstructorsCommand>
    {
        private readonly ILmsDbContext _context;

        public AssignCourseInstructorsCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.CourseIdRequired].Value)
                .MustAsync((courseId, cancellationToken) =>
                    CoursesValidation.CourseExistsAsync(_context, courseId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.CourseNotFound].Value);

            RuleFor(command => command.InstructorIds)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage(localizer[LocalizationKeys.Courses.InstructorIdRequired].Value)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage(localizer[LocalizationKeys.Courses.DuplicateInstructors].Value)
                .MustAsync((ids, cancellationToken) =>
                    CoursesValidation.InstructorsAreAssignableAsync(_context, ids, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.InstructorNotAssignable].Value);
        }
    }
}
