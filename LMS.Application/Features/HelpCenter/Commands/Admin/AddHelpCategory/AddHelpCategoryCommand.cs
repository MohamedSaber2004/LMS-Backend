using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpCategory
{
    public class AddHelpCategoryCommand : IRequest<Result<HelpCategoryDto>>
    {
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
