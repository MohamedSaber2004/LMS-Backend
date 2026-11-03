using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommandHandler
        : IRequestHandler<UpdateHelpCategoryCommand, Result<HelpCategoryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateHelpCategoryCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<HelpCategoryDto>> Handle(
            UpdateHelpCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _context.HelpCategories
                .Include(item => item.Articles)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.HelpCenter.CategoryNotFound);

            category.Update(request.Name.Trim(), request.Order);
            category.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            return Result<HelpCategoryDto>.Success(
                HelpCenterMapper.ToCategoryDto(category),
                _localizer[LocalizationKeys.HelpCenter.CategoryUpdatedSuccess].Value);
        }
    }
}
