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

namespace LMS.Application.Features.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public ChangeUserPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
            if (!resetResult.Succeeded)
            {
                return BuildIdentityFailure(resetResult);
            }

            user.IncrementTokenVersion();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BuildIdentityFailure(updateResult);
            }

            await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Users.PasswordChangedSuccess].Value;
            return Result<string>.Success(message, message);
        }

        private Result<string> BuildIdentityFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToArray();

            return Result<string>.Failure(
                _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value,
                StatusCodes.Status400BadRequest,
                errors);
        }
    }
}
