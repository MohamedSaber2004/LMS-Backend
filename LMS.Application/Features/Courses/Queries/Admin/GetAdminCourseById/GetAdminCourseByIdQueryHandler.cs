using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.Admin.GetAdminCourseById
{
    public class GetAdminCourseByIdQueryHandler
        : IRequestHandler<GetAdminCourseByIdQuery, Result<CourseAdminDetailDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminCourseByIdQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<CourseAdminDetailDto>> Handle(
            GetAdminCourseByIdQuery request,
            CancellationToken cancellationToken)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .Include(item => item.Lessons)
                .ThenInclude(lesson => lesson.QuizQuestions)
                .ThenInclude(question => question.Options)
                .IncludeAssignedInstructors()
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.CourseNotFound);

            return Result<CourseAdminDetailDto>.Success(
                CoursesMapper.ToAdminDetailDto(course),
                _localizer[LocalizationKeys.Courses.CourseRetrieved].Value);
        }
    }
}
