using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(
            IJwtTokenService jwtTokenService,
            ILmsDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.RefreshTokenRequired].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, _, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.RefreshToken))
                    {
                        return;
                    }

                    var userId = jwtTokenService.ValidateRefreshToken(command.RefreshToken);
                    if (userId is null)
                    {
                        throw new UnAuthorizedException(LocalizationKeys.ExceptionMessages.TokenInvalid);
                    }

                    var storedToken = await context.UserRefreshTokens
                        .FirstOrDefaultAsync(
                            token => token.Token == command.RefreshToken && token.UserId == userId.Value,
                            cancellationToken);

                    if (storedToken is null)
                    {
                        throw new UnAuthorizedException(LocalizationKeys.ExceptionMessages.TokenInvalid);
                    }

                    if (storedToken.IsRevoked)
                    {
                        throw new UnAuthorizedException(LocalizationKeys.ExceptionMessages.TokenRevoked);
                    }

                    if (storedToken.ExpiryDate <= DateTime.UtcNow)
                    {
                        throw new UnAuthorizedException(LocalizationKeys.ExceptionMessages.TokenExpired);
                    }

                    var user = await userManager.FindByIdAsync(userId.Value.ToString());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }
                });
        }
    }
}
