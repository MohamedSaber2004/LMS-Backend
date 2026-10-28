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

namespace LMS.Application.Features.Authentication.Commands.ChangetPassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ChangePasswordCommandHandler(
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

        public async Task<Result<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString())
                ?? throw new UnAuthorizedException();

            var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!changeResult.Succeeded)
            {
                var errors = changeResult.Errors.Select(e => e.Description).ToArray();
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.Authentication.PasswordRequirementsNotMet].Value,
                    StatusCodes.Status400BadRequest,
                    errors);
            }

            user.IncrementTokenVersion();
            await _userManager.UpdateAsync(user);

            await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.ChangePasswordSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
