using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Courses
{
    internal static class CoursesAccess
    {
        public static async Task<Guid> RequireLearnerIdAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken)
        {
            return await ActorOwnership.ResolveLearnerIdAsync(context, currentUser.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
        }

        public static async Task RequireLessonAccessAsync(
            ILmsDbContext context,
            Lesson lesson,
            Guid? learnerId,
            CancellationToken cancellationToken)
        {
            if (lesson.IsFreeDemo || lesson.Order == 1)
            {
                return;
            }

            var isFirstLesson = !await context.Lessons
                .AnyAsync(l => l.CourseId == lesson.CourseId && l.Order < lesson.Order, cancellationToken);
            if (isFirstLesson)
            {
                return;
            }

            if (learnerId is null
                || !await CoursesValidation.LearnerIsEnrolledAsync(
                    context, lesson.CourseId, learnerId.Value, cancellationToken))
            {
                throw new ForbiddenException(LocalizationKeys.Courses.LessonLocked);
            }
        }
    }
}
