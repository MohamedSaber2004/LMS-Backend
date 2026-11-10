using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.Admin.GetCoursesPagginated
{
    public class GetCoursesPagginatedQueryHandler
        : IRequestHandler<GetCoursesPagginatedQuery, Result<PagginatedResult<CourseListItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetCoursesPagginatedQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<CourseListItemDto>>> Handle(
            GetCoursesPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Courses.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(course =>
                    course.Title.Contains(search)
                    || course.Description.Contains(search));
            }

            if (request.Level.HasValue)
            {
                var level = request.Level.Value;
                query = query.Where(course => course.Level == level);
            }

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value;
                query = query.Where(course => course.IsActive == isActive);
            }

            if (request.InstructorId.HasValue)
            {
                var instructorId = request.InstructorId.Value;
                query = query.Where(course => course.CourseInstructors
                    .Any(courseInstructor => courseInstructor.InstructorId == instructorId));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<CourseListItemDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<CourseListItemDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<CourseListItemDto>.MaxPageSize);

            var items = await query
                .OrderBy(course => course.Order)
                .ThenBy(course => course.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(course => new CourseListItemDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    CoverUrl = course.CoverUrl,
                    Level = course.Level,
                    Order = course.Order,
                    Price = course.Price,
                    IsActive = course.IsActive,
                    LessonCount = course.Lessons.Count(lesson => lesson.IsActive),
                    Instructors = course.CourseInstructors
                        .OrderBy(courseInstructor => courseInstructor.CreatedAt)
                        .Select(courseInstructor => new InstructorRefDto
                        {
                            Id = courseInstructor.InstructorId,
                            UserId = courseInstructor.Instructor.UserId,
                            FullName = courseInstructor.Instructor.User.FullName,
                            Specialization = courseInstructor.Instructor.Specialization
                        })
                        .ToArray()
                })
                .ToListAsync(cancellationToken);

            var result = PagginatedResult<CourseListItemDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<CourseListItemDto>>.Success(
                result,
                _localizer[LocalizationKeys.Courses.CoursesRetrieved].Value);
        }
    }
}
