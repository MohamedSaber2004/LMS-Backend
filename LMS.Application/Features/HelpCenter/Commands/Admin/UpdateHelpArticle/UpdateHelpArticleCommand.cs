using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpArticle
{
    public class UpdateHelpArticleCommand : IRequest<Result<HelpArticleDto>>
    {
        public Guid Id { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
