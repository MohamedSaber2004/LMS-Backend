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

namespace LMS.Application.Features.Courses.Queries.GetLessonContent
{
    public class GetLessonContentQueryHandler
        : IRequestHandler<GetLessonContentQuery, Result<LessonContentDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetLessonContentQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<LessonContentDto>> Handle(
            GetLessonContentQuery request,
            CancellationToken cancellationToken)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(item => item.Course)
                .Include(item => item.QuizQuestions)
                .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(
                    item => item.Id == request.LessonId && item.Course.IsActive,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.LessonNotFound);

            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            await CoursesAccess.RequireLessonAccessAsync(_context, lesson, learnerId, cancellationToken);

            LessonProgressDto? progress = null;
            QuizResultDto? lastAttempt = null;

            if (learnerId is not null)
            {
                var progressRow = await _context.LessonProgress
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        item => item.LessonId == lesson.Id && item.LearnerId == learnerId.Value,
                        cancellationToken);

                progress = progressRow is null ? null : CoursesMapper.ToProgressDto(progressRow);

                var attempt = await _context.QuizAttempts
                    .AsNoTracking()
                    .Include(item => item.Answers)
                    .Where(item => item.LessonId == lesson.Id && item.LearnerId == learnerId.Value)
                    .OrderByDescending(item => item.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                lastAttempt = attempt is null ? null : CoursesMapper.ToResultDto(attempt);
            }

            var content = new LessonContentDto
            {
                CourseId = lesson.CourseId,
                LessonId = lesson.Id,
                CourseTitle = lesson.Course.Title,
                Title = lesson.Title,
                Order = lesson.Order,
                IsFreeDemo = lesson.IsFreeDemo,
                VideoUrl = lesson.VideoUrl,
                StudyBookUrl = lesson.StudyBookUrl,
                Progress = progress,
                LastAttempt = lastAttempt,
                QuizQuestions = lesson.QuizQuestions
                    .Where(question => question.IsActive)
                    .OrderBy(question => question.Order)
                    .Select(CoursesMapper.ToQuestionDto)
                    .ToArray()
            };

            return Result<LessonContentDto>.Success(
                content,
                _localizer[LocalizationKeys.Courses.LessonRetrieved].Value);
        }
    }
}
