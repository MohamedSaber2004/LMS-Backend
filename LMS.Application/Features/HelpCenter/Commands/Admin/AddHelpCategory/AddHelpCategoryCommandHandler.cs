using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpCategory
{
    public class AddHelpCategoryCommandHandler
        : IRequestHandler<AddHelpCategoryCommand, Result<HelpCategoryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddHelpCategoryCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<HelpCategoryDto>> Handle(
            AddHelpCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = HelpCategory.Create(request.Name.Trim(), request.Order);

            await _context.HelpCategories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<HelpCategoryDto>.Success(
                HelpCenterMapper.ToCategoryDto(category),
                _localizer[LocalizationKeys.HelpCenter.CategoryCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
