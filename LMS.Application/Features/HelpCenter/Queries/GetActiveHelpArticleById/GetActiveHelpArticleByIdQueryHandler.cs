using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticleById
{
    public class GetActiveHelpArticleByIdQueryHandler
        : IRequestHandler<GetActiveHelpArticleByIdQuery, Result<HelpArticleDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetActiveHelpArticleByIdQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<HelpArticleDto>> Handle(
            GetActiveHelpArticleByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Unpublished rows answer 404 rather than 403 so hidden content cannot be enumerated.
            var article = await _context.HelpArticles
                .AsNoTracking()
                .Include(item => item.Category)
                .FirstOrDefaultAsync(
                    item => item.Id == request.Id
                        && item.IsActive
                        && item.Category.IsActive,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.HelpCenter.ArticleNotFound);

            return Result<HelpArticleDto>.Success(
                HelpCenterMapper.ToArticleDto(article),
                _localizer[LocalizationKeys.HelpCenter.ArticleRetrieved].Value);
        }
    }
}
