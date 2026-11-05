using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetActiveCourseById
{
    public class GetActiveCourseByIdQueryHandler
        : IRequestHandler<GetActiveCourseByIdQuery, Result<CourseDetailDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetActiveCourseByIdQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<CourseDetailDto>> Handle(
            GetActiveCourseByIdQuery request,
            CancellationToken cancellationToken)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .Include(item => item.Lessons)
                .IncludeAssignedInstructors()
                .Where(item => item.IsActive)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.CourseNotFound);

            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            var isEnrolled = learnerId is not null
                && await CoursesValidation.LearnerIsEnrolledAsync(
                    _context, course.Id, learnerId.Value, cancellationToken);

            return Result<CourseDetailDto>.Success(
                CoursesMapper.ToDetailDto(course, isEnrolled),
                _localizer[LocalizationKeys.Courses.CourseRetrieved].Value);
        }
    }
}
