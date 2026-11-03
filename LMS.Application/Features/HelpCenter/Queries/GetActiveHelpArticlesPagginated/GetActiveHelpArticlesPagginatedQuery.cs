using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticlesPagginated
{
    public class GetActiveHelpArticlesPagginatedQuery
        : IRequest<Result<PagginatedResult<HelpArticleDto>>>
    {
        public GetActiveHelpArticlesPagginatedQuery() { }

        public Guid? CategoryId { get; set; }

        public string? Search { get; set; }

        public int PageNumber { get; set; } = PagginatedResult<HelpArticleDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<HelpArticleDto>.DefaultPageSize;
    }
}
