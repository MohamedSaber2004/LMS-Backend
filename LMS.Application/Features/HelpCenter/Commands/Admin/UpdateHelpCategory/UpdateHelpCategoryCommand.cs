using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommand : IRequest<Result<HelpCategoryDto>>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
