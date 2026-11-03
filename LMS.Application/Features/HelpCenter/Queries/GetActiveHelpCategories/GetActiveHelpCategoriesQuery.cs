using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using MediatR;

namespace LMS.Application.Features.HelpCenter.Queries.GetActiveHelpCategories
{
    public class GetActiveHelpCategoriesQuery
        : IRequest<Result<IReadOnlyCollection<HelpCategoryDto>>>
    {
        public GetActiveHelpCategoriesQuery() { }
    }
}
