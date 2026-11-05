using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.EnrollCourse
{
    public class EnrollCourseCommandHandler : IRequestHandler<EnrollCourseCommand, Result<CourseDetailDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public EnrollCourseCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<CourseDetailDto>> Handle(
            EnrollCourseCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await CoursesAccess.RequireLearnerIdAsync(
                _context, _currentUserService, cancellationToken);

            var course = await _context.Courses
                .AsNoTracking()
                .Include(item => item.Lessons)
                .IncludeAssignedInstructors()
                .FirstOrDefaultAsync(
                    item => item.Id == request.CourseId && item.IsActive,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.CourseNotFound);

            var alreadyEnrolled = await CoursesValidation.LearnerIsEnrolledAsync(
                _context, course.Id, learnerId, cancellationToken);

            if (!alreadyEnrolled)
            {
                await _context.CourseEnrollments.AddAsync(
                    CourseEnrollment.Create(course.Id, learnerId), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var messageKey = alreadyEnrolled
                ? LocalizationKeys.Courses.AlreadyEnrolled
                : LocalizationKeys.Courses.EnrolledSuccess;

            return Result<CourseDetailDto>.Success(
                CoursesMapper.ToDetailDto(course, true),
                _localizer[messageKey].Value);
        }
    }
}
