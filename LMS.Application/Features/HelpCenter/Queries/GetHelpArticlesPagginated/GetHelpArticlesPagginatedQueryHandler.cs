using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Queries.GetHelpArticlesPagginated
{
    public class GetHelpArticlesPagginatedQueryHandler
        : IRequestHandler<GetHelpArticlesPagginatedQuery, Result<PagginatedResult<HelpArticleDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetHelpArticlesPagginatedQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<HelpArticleDto>>> Handle(
            GetHelpArticlesPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            // Admin view: unpublished articles are listed too, optionally narrowed by IsActive.
            var query = _context.HelpArticles
                .AsNoTracking()
                .Include(article => article.Category)
                .AsQueryable();

            if (request.CategoryId.HasValue)
            {
                var categoryId = request.CategoryId.Value;
                query = query.Where(article => article.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(article =>
                    article.Question.Contains(search)
                    || article.Answer.Contains(search));
            }

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value;
                query = query.Where(article => article.IsActive == isActive);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<HelpArticleDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<HelpArticleDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<HelpArticleDto>.MaxPageSize);

            var articles = await query
                .OrderBy(article => article.Category.Name)
                .ThenBy(article => article.Order)
                .ThenBy(article => article.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = articles
                .Select(article => HelpCenterMapper.ToArticleDto(article))
                .ToArray();

            var result = PagginatedResult<HelpArticleDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<HelpArticleDto>>.Success(
                result,
                _localizer[LocalizationKeys.HelpCenter.ArticlesRetrieved].Value);
        }
    }
}
