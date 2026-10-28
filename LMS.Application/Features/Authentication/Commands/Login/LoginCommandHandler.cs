using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using JwtSettings = LMS.Application.Common.Options.JwtSettings;

namespace LMS.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILmsDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IStringLocalizer<Messages> _localizer;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            ILmsDbContext context,
            IOptions<JwtSettings> jwtOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _context = context;
            _jwtSettings = jwtOptions?.Value ?? new JwtSettings();
            _localizer = localizer;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                throw new UnAuthorizedException();
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user);

            var refreshDays = _jwtSettings.RefreshTokenExpiryDays > 0 ? _jwtSettings.RefreshTokenExpiryDays : 30;
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, refreshTokenExpiry);
            await _context.UserRefreshTokens.AddAsync(userRefreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponseDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                accessToken,
                refreshToken,
                refreshTokenExpiry);

            return Result<AuthResponseDto>.Success(
                response,
                _localizer[LocalizationKeys.Authentication.LoginSuccess].Value);
        }
    }
}
