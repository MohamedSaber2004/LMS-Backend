using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserAccountDeletionService _accountDeletionService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUserAccountDeletionService accountDeletionService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _accountDeletionService = accountDeletionService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString())
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            await _accountDeletionService.HardDeleteUserAsync(user.Id, cancellationToken);

            var message = _localizer[LocalizationKeys.Users.UserDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
