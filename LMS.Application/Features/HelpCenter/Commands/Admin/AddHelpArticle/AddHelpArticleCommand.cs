using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpArticle
{
    public class AddHelpArticleCommand : IRequest<Result<HelpArticleDto>>
    {
        public Guid CategoryId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
