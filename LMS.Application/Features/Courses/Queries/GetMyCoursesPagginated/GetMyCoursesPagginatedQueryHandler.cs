using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.GetMyCoursesPagginated
{
    public class GetMyCoursesPagginatedQueryHandler
        : IRequestHandler<GetMyCoursesPagginatedQuery, Result<PagginatedResult<MyCourseDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyCoursesPagginatedQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<MyCourseDto>>> Handle(
            GetMyCoursesPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            var learnerId = await CoursesAccess.RequireLearnerIdAsync(
                _context, _currentUserService, cancellationToken);

            var query = _context.CourseEnrollments
                .AsNoTracking()
                .Where(enrollment => enrollment.LearnerId == learnerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<MyCourseDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<MyCourseDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<MyCourseDto>.MaxPageSize);

            var enrollments = await query
                .OrderByDescending(enrollment => enrollment.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(enrollment => new
                {
                    enrollment.CourseId,
                    enrollment.CreatedAt,
                    enrollment.Course.Title,
                    enrollment.Course.Description,
                    enrollment.Course.CoverUrl,
                    enrollment.Course.Level,
                    LessonCount = enrollment.Course.Lessons.Count(lesson => lesson.IsActive),
                    Instructors = enrollment.Course.CourseInstructors
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

            var courseIds = enrollments.Select(enrollment => enrollment.CourseId).ToArray();

            var progressRows = await _context.LessonProgress
                .AsNoTracking()
                .Where(progress => progress.LearnerId == learnerId
                    && courseIds.Contains(progress.Lesson.CourseId))
                .Select(progress => new
                {
                    CourseId = progress.Lesson.CourseId,
                    progress.LessonId,
                    LessonTitle = progress.Lesson.Title,
                    progress.IsCompleted,
                    progress.LastWatchedSeconds,
                    progress.CreatedAt,
                    progress.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var items = new List<MyCourseDto>(enrollments.Count);

            foreach (var enrollment in enrollments)
            {
                var rows = progressRows
                    .Where(row => row.CourseId == enrollment.CourseId)
                    .ToList();

                var last = rows
                    .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
                    .FirstOrDefault();

                items.Add(new MyCourseDto
                {
                    CourseId = enrollment.CourseId,
                    Title = enrollment.Title,
                    Description = enrollment.Description,
                    CoverUrl = enrollment.CoverUrl,
                    Level = enrollment.Level,
                    LessonCount = enrollment.LessonCount,
                    Instructors = enrollment.Instructors,
                    CompletedLessonCount = rows.Count(row => row.IsCompleted),
                    LastLessonId = last?.LessonId,
                    LastLessonTitle = last?.LessonTitle,
                    LastWatchedSeconds = last?.LastWatchedSeconds,
                    LastLessonIsCompleted = last?.IsCompleted,
                    EnrolledAtUtc = enrollment.CreatedAt
                });
            }

            var result = PagginatedResult<MyCourseDto>.Create(items.ToArray(), totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<MyCourseDto>>.Success(
                result,
                _localizer[LocalizationKeys.Courses.MyCoursesRetrieved].Value);
        }
    }
}
