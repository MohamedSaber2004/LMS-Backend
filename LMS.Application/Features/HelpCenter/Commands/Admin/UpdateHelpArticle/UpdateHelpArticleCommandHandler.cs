using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpArticle
{
    public class UpdateHelpArticleCommandHandler
        : IRequestHandler<UpdateHelpArticleCommand, Result<HelpArticleDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateHelpArticleCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<HelpArticleDto>> Handle(
            UpdateHelpArticleCommand request,
            CancellationToken cancellationToken)
        {
            var article = await _context.HelpArticles
                .Include(item => item.Category)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.HelpCenter.ArticleNotFound);

            article.Update(request.Question.Trim(), request.Answer, request.Order);
            article.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            return Result<HelpArticleDto>.Success(
                HelpCenterMapper.ToArticleDto(article),
                _localizer[LocalizationKeys.HelpCenter.ArticleUpdatedSuccess].Value);
        }
    }
}
