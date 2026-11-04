using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpArticle
{
    public class AddHelpArticleCommandHandler
        : IRequestHandler<AddHelpArticleCommand, Result<HelpArticleDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddHelpArticleCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<HelpArticleDto>> Handle(
            AddHelpArticleCommand request,
            CancellationToken cancellationToken)
        {
            var categoryName = await _context.HelpCategories
                .Where(category => category.Id == request.CategoryId)
                .Select(category => category.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            var article = HelpArticle.Create(
                request.CategoryId,
                request.Question.Trim(),
                request.Answer,
                request.Order);

            await _context.HelpArticles.AddAsync(article, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<HelpArticleDto>.Success(
                HelpCenterMapper.ToArticleDto(article, categoryName),
                _localizer[LocalizationKeys.HelpCenter.ArticleCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
