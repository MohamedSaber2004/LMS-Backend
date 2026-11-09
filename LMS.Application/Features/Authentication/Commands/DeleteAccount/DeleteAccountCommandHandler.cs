using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserAccountDeletionService _accountDeletionService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteAccountCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUserAccountDeletionService accountDeletionService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _accountDeletionService = accountDeletionService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString())
                ?? throw new UnAuthorizedException();

            await _accountDeletionService.HardDeleteUserAsync(user.Id, cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.AccountDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
