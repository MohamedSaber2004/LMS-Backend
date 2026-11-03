using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Queries.GetHelpCategories
{
    public class GetHelpCategoriesQueryHandler
        : IRequestHandler<GetHelpCategoriesQuery, Result<IReadOnlyCollection<HelpCategoryDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetHelpCategoriesQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<HelpCategoryDto>>> Handle(
            GetHelpCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            // Admin view: inactive categories stay visible so they can be re-enabled.
            var categories = await _context.HelpCategories
                .AsNoTracking()
                .Include(category => category.Articles)
                .OrderBy(category => category.Order)
                .ThenBy(category => category.CreatedAt)
                .ToListAsync(cancellationToken);

            var items = categories
                .Select(category => HelpCenterMapper.ToCategoryDto(category))
                .ToArray();

            return Result<IReadOnlyCollection<HelpCategoryDto>>.Success(
                items,
                _localizer[LocalizationKeys.HelpCenter.CategoriesRetrieved].Value);
        }
    }
}
