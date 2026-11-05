using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Courses
{
    internal static class CoursesValidation
    {
        public static async Task<bool> CourseExistsAsync(
            ILmsDbContext context,
            Guid courseId,
            CancellationToken cancellationToken)
        {
            return courseId != Guid.Empty
                && await context.Courses.AnyAsync(course => course.Id == courseId, cancellationToken);
        }

        public static async Task<bool> LessonExistsAsync(
            ILmsDbContext context,
            Guid lessonId,
            CancellationToken cancellationToken)
        {
            return lessonId != Guid.Empty
                && await context.Lessons.AnyAsync(lesson => lesson.Id == lessonId, cancellationToken);
        }

        public static async Task<bool> QuestionExistsAsync(
            ILmsDbContext context,
            Guid questionId,
            CancellationToken cancellationToken)
        {
            return questionId != Guid.Empty
                && await context.QuizQuestions.AnyAsync(question => question.Id == questionId, cancellationToken);
        }

        public static async Task<bool> LearnerIsEnrolledAsync(
            ILmsDbContext context,
            Guid courseId,
            Guid learnerId,
            CancellationToken cancellationToken)
        {
            return courseId != Guid.Empty
                && learnerId != Guid.Empty
                && await context.CourseEnrollments.AnyAsync(
                    enrollment => enrollment.CourseId == courseId && enrollment.LearnerId == learnerId,
                    cancellationToken);
        }
        public static async Task<bool> InstructorsAreAssignableAsync(
            ILmsDbContext context,
            IEnumerable<Guid> instructorIds,
            CancellationToken cancellationToken)
        {
            var ids = instructorIds.Distinct().ToArray();

            if (ids.Length == 0)
            {
                return true;
            }

            var assignableCount = await context.Instructors
                .Where(instructor => ids.Contains(instructor.Id)
                    && instructor.IsActive
                    && instructor.Application != null
                    && instructor.Application.Status == InstructorApplicationStatus.Approved
                    && instructor.User.IsActive
                    && !instructor.User.IsDeleted)
                .CountAsync(cancellationToken);

            return assignableCount == ids.Length;
        }
    }
}
