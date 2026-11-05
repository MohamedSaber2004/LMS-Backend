using LMS.Application.Common.Interfaces;
using LMS.Application.Features.Courses.DTOs;
using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Courses
{
    internal static class CoursesInstructors
    {
        public static IQueryable<Course> IncludeAssignedInstructors(this IQueryable<Course> query)
        {
            return query
                .Include(course => course.CourseInstructors)
                .ThenInclude(courseInstructor => courseInstructor.Instructor)
                .ThenInclude(instructor => instructor.User);
        }

        public static async Task<IReadOnlyCollection<InstructorRefDto>> ForCourseAsync(
            ILmsDbContext context,
            Guid courseId,
            CancellationToken cancellationToken)
        {
            return await context.CourseInstructors
                .AsNoTracking()
                .Where(courseInstructor => courseInstructor.CourseId == courseId)
                .OrderBy(courseInstructor => courseInstructor.CreatedAt)
                .Select(courseInstructor => new InstructorRefDto
                {
                    Id = courseInstructor.InstructorId,
                    UserId = courseInstructor.Instructor.UserId,
                    FullName = courseInstructor.Instructor.User.FullName,
                    Specialization = courseInstructor.Instructor.Specialization
                })
                .ToListAsync(cancellationToken);
        }
    }
}
