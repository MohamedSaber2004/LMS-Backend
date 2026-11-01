using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.DeleteOnboardingOption
{
    public class DeleteOnboardingOptionCommandHandler
        : IRequestHandler<DeleteOnboardingOptionCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteOnboardingOptionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            DeleteOnboardingOptionCommand request,
            CancellationToken cancellationToken)
        {
            var option = await _context.OnboardingOptions
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Onboarding.OptionNotFound);

            option.MarkAsDeleted(_currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Onboarding.OptionDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
