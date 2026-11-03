using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpCategory
{
    public class DeleteHelpCategoryCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
