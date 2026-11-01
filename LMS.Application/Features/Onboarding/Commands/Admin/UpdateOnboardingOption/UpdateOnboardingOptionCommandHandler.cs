using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingOption
{
    public class UpdateOnboardingOptionCommandHandler
        : IRequestHandler<UpdateOnboardingOptionCommand, Result<OnboardingOptionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateOnboardingOptionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<OnboardingOptionDto>> Handle(
            UpdateOnboardingOptionCommand request,
            CancellationToken cancellationToken)
        {
            var option = await _context.OnboardingOptions
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Onboarding.OptionNotFound);

            option.Label = request.Label.Trim();
            option.Value = request.Value.Trim();
            option.Order = request.Order;
            option.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnboardingOptionDto
            {
                Id = option.Id,
                Label = option.Label,
                Value = option.Value,
                Order = option.Order,
                IsActive = option.IsActive
            };

            return Result<OnboardingOptionDto>.Success(
                response,
                _localizer[LocalizationKeys.Onboarding.OptionUpdatedSuccess].Value);
        }
    }
}
