using LMS.Application.Features.Courses.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.Courses
{
    public static class CoursesMapper
    {
        public static CourseListItemDto ToListItemDto(Course course, int lessonCount) => new()
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CoverUrl = course.CoverUrl,
            Level = course.Level,
            Order = course.Order,
            LessonCount = lessonCount,
            IsActive = course.IsActive
        };

        public static CourseDetailDto ToDetailDto(Course course, bool isEnrolled) => new()
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CoverUrl = course.CoverUrl,
            Level = course.Level,
            Order = course.Order,
            IsEnrolled = isEnrolled,
            Lessons = course.Lessons
                .Where(lesson => lesson.IsActive)
                .OrderBy(lesson => lesson.Order)
                .Select(ToLessonListItemDto)
                .ToArray()
        };

        public static LessonListItemDto ToLessonListItemDto(Lesson lesson) => new()
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Order = lesson.Order,
            IsFreeDemo = lesson.IsFreeDemo,
            HasStudyBook = !string.IsNullOrWhiteSpace(lesson.StudyBookUrl)
        };

        public static CourseAdminDetailDto ToAdminDetailDto(Course course) => new()
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CoverUrl = course.CoverUrl,
            Level = course.Level,
            Order = course.Order,
            IsActive = course.IsActive,
            Lessons = course.Lessons
                .OrderBy(lesson => lesson.Order)
                .Select(ToAdminLessonDto)
                .ToArray()
        };

        public static LessonAdminDto ToAdminLessonDto(Lesson lesson) => new()
        {
            Id = lesson.Id,
            Title = lesson.Title,
            VideoUrl = lesson.VideoUrl,
            StudyBookUrl = lesson.StudyBookUrl,
            Order = lesson.Order,
            IsFreeDemo = lesson.IsFreeDemo,
            IsActive = lesson.IsActive,
            QuizQuestions = lesson.QuizQuestions
                .OrderBy(question => question.Order)
                .Select(ToAdminQuestionDto)
                .ToArray()
        };

        public static QuizQuestionAdminDto ToAdminQuestionDto(QuizQuestion question) => new()
        {
            Id = question.Id,
            Text = question.Text,
            Order = question.Order,
            IsActive = question.IsActive,
            Options = question.Options
                .OrderBy(option => option.Order)
                .Select(option => new QuizOptionAdminDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    IsCorrect = option.IsCorrect,
                    Order = option.Order
                })
                .ToArray()
        };

        public static QuizQuestionDto ToQuestionDto(QuizQuestion question) => new()
        {
            Id = question.Id,
            Text = question.Text,
            Order = question.Order,
            Options = question.Options
                .Where(option => option.IsActive)
                .OrderBy(option => option.Order)
                .Select(option => new QuizOptionDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    Order = option.Order
                })
                .ToArray()
        };

        public static LessonProgressDto ToProgressDto(LessonProgress progress) => new()
        {
            LastWatchedSeconds = progress.LastWatchedSeconds,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt
        };

        public static QuizResultDto ToResultDto(QuizAttempt attempt) => new()
        {
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            SubmittedAtUtc = attempt.CreatedAt,
            Answers = attempt.Answers
                .Select(answer => new QuizAttemptAnswerDto
                {
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId
                })
                .ToArray()
        };
    }
}
