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

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateQuizQuestion
{
    public class UpdateQuizQuestionCommandHandler
        : IRequestHandler<UpdateQuizQuestionCommand, Result<QuizQuestionAdminDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateQuizQuestionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<QuizQuestionAdminDto>> Handle(
            UpdateQuizQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var question = await _context.QuizQuestions
                .Include(item => item.Options)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.QuestionNotFound);

            question.Update(request.Text.Trim(), request.Order);

            question.ReplaceOptions(request.Options!
                .Select(option => QuizOption.Create(
                    question.Id,
                    option.Text.Trim(),
                    option.IsCorrect,
                    option.Order)));

            question.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            return Result<QuizQuestionAdminDto>.Success(
                CoursesMapper.ToAdminQuestionDto(question),
                _localizer[LocalizationKeys.Courses.QuestionUpdatedSuccess].Value);
        }
    }
}
