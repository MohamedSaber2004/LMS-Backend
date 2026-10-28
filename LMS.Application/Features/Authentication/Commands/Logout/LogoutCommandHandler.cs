using LMS.API.Services;
using LMS.Application.Common.Extensions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public LogoutCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var refreshToken = await _context.UserRefreshTokens
                    .FirstOrDefaultAsync(token => token.Token == request.RefreshToken && token.UserId == userId, cancellationToken);

                refreshToken?.Revoke();
            }
            else
            {
                await _context.RevokeAllRefreshTokensAsync(userId, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.LogoutSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
