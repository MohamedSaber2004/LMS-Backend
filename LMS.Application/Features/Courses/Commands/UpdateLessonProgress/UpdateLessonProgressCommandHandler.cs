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

namespace LMS.Application.Features.Courses.Commands.UpdateLessonProgress
{
    public class UpdateLessonProgressCommandHandler
        : IRequestHandler<UpdateLessonProgressCommand, Result<LessonProgressDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateLessonProgressCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<LessonProgressDto>> Handle(
            UpdateLessonProgressCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await CoursesAccess.RequireLearnerIdAsync(
                _context, _currentUserService, cancellationToken);

            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(item => item.Course)
                .FirstOrDefaultAsync(
                    item => item.Id == request.LessonId && item.Course.IsActive,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.LessonNotFound);

            await CoursesAccess.RequireLessonAccessAsync(
                _context, lesson, learnerId, cancellationToken);

            var progress = await _context.LessonProgress
                .FirstOrDefaultAsync(
                    item => item.LessonId == lesson.Id && item.LearnerId == learnerId,
                    cancellationToken);

            if (progress is null)
            {
                progress = LessonProgress.Create(
                    lesson.Id,
                    learnerId,
                    request.LastWatchedSeconds,
                    request.IsCompleted);

                await _context.LessonProgress.AddAsync(progress, cancellationToken);
            }
            else
            {
                progress.Record(request.LastWatchedSeconds, request.IsCompleted);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<LessonProgressDto>.Success(
                CoursesMapper.ToProgressDto(progress),
                _localizer[LocalizationKeys.Courses.ProgressUpdatedSuccess].Value);
        }
    }
}
