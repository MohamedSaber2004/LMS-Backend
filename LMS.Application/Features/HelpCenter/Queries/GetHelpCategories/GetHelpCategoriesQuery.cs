using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Queries.GetHelpCategories
{
    public class GetHelpCategoriesQuery : IRequest<Result<IReadOnlyCollection<HelpCategoryDto>>>
    {
        public GetHelpCategoriesQuery() { }
    }
}
