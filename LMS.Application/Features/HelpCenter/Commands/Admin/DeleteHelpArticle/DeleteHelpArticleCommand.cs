using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpArticle
{
    public class DeleteHelpArticleCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
