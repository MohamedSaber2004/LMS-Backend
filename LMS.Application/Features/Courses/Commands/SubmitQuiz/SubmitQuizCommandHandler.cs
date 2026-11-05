using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.SubmitQuiz
{
    public class SubmitQuizCommandHandler : IRequestHandler<SubmitQuizCommand, Result<QuizResultDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SubmitQuizCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<QuizResultDto>> Handle(
            SubmitQuizCommand request,
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

            var questions = await _context.QuizQuestions
                .AsNoTracking()
                .Include(item => item.Options)
                .Where(item => item.LessonId == lesson.Id && item.IsActive)
                .ToListAsync(cancellationToken);

            var answers = request.Answers!;

            var selectedOptions = answers
                .ToDictionary(answer => answer.QuestionId, answer => answer.SelectedOptionId);

            var score = questions.Count(question =>
                selectedOptions.TryGetValue(question.Id, out var selectedOptionId)
                && question.Options.Any(option =>
                    option.IsActive
                    && option.Id == selectedOptionId
                    && option.IsCorrect));

            var attempt = QuizAttempt.Create(lesson.Id, learnerId, score, questions.Count);

            foreach (var answer in answers)
            {
                attempt.AddAnswer(answer.QuestionId, answer.SelectedOptionId);
            }

            await _context.QuizAttempts.AddAsync(attempt, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<QuizResultDto>.Success(
                CoursesMapper.ToResultDto(attempt),
                _localizer[LocalizationKeys.Courses.QuizSubmittedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
