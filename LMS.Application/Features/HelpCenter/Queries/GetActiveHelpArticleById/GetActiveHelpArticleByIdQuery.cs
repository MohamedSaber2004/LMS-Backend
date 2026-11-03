using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticleById
{
    public class GetActiveHelpArticleByIdQuery : IRequest<Result<HelpArticleDto>>
    {
        public GetActiveHelpArticleByIdQuery() { }

        public Guid Id { get; set; }
    }
}
