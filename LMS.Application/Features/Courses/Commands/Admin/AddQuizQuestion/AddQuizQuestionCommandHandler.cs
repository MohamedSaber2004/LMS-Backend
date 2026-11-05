using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AddQuizQuestion
{
    public class AddQuizQuestionCommandHandler : IRequestHandler<AddQuizQuestionCommand, Result<QuizQuestionAdminDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddQuizQuestionCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<QuizQuestionAdminDto>> Handle(
            AddQuizQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var question = QuizQuestion.Create(request.LessonId, request.Text.Trim(), request.Order);

            question.ReplaceOptions(request.Options!
                .Select(option => QuizOption.Create(
                    question.Id,
                    option.Text.Trim(),
                    option.IsCorrect,
                    option.Order)));

            await _context.QuizQuestions.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<QuizQuestionAdminDto>.Success(
                CoursesMapper.ToAdminQuestionDto(question),
                _localizer[LocalizationKeys.Courses.QuestionCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
