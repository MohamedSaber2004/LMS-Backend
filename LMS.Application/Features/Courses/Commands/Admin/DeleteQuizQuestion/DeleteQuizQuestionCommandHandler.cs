using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteQuizQuestion
{
    public class DeleteQuizQuestionCommandHandler
        : IRequestHandler<DeleteQuizQuestionCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteQuizQuestionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            DeleteQuizQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var deletedBy = _currentUserService.UserId.ToString();

            var question = await _context.QuizQuestions
                .Include(item => item.Options)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.QuestionNotFound);

            foreach (var option in question.Options)
            {
                option.MarkAsDeleted(deletedBy);
            }

            question.MarkAsDeleted(deletedBy);

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Courses.QuestionDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
