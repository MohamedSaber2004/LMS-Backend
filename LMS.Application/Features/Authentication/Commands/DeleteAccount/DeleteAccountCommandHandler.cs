using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Extensions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteAccountCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString())
                ?? throw new UnAuthorizedException();

            user.MarkAsDeleted();
            user.IncrementTokenVersion();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToArray();
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.ExceptionMessages.InternalServerError].Value,
                    StatusCodes.Status500InternalServerError,
                    errors);
            }

            await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.AccountDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
