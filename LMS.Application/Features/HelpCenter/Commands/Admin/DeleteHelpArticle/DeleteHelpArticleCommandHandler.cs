using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpArticle
{
    public class DeleteHelpArticleCommandHandler
        : IRequestHandler<DeleteHelpArticleCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteHelpArticleCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            DeleteHelpArticleCommand request,
            CancellationToken cancellationToken)
        {
            var article = await _context.HelpArticles
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.HelpCenter.ArticleNotFound);

            article.MarkAsDeleted(_currentUserService.UserId.ToString());
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.HelpCenter.ArticleDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
