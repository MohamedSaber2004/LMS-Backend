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

namespace LMS.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public ResetPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim())
                ?? throw new UnAuthorizedException();

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!resetResult.Succeeded)
            {
                var errors = resetResult.Errors.Select(e => e.Description).ToArray();
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value,
                    StatusCodes.Status400BadRequest,
                    errors);
            }

            user.ClearVerificationCode();
            user.IncrementTokenVersion();
            await _userManager.UpdateAsync(user);
            await _userManager.ResetAccessFailedCountAsync(user);

            await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.PasswordResetSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
