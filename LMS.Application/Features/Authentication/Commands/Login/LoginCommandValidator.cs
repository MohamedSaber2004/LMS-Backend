using System.ComponentModel.DataAnnotations;
using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using AppIdentityOptions = LMS.Application.Common.Options.IdentityOptions;

namespace LMS.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        private static readonly EmailAddressAttribute EmailValidator = new();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppIdentityOptions _identityOptions;

        public LoginCommandValidator(
            UserManager<ApplicationUser> userManager,
            IOptions<AppIdentityOptions> identityOptions)
        {
            _userManager = userManager;
            _identityOptions = identityOptions?.Value ?? new AppIdentityOptions();

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.Email) ||
                        string.IsNullOrWhiteSpace(command.Password) ||
                        !EmailValidator.IsValid(command.Email))
                    {
                        throw new UnAuthorizedException();
                    }

                    var user = await _userManager.FindByEmailAsync(command.Email);
                    if (user == null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        throw new UnAuthorizedException();
                    }

                    if (_identityOptions.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        throw new UnAuthorizedException();
                    }

                    var isPasswordValid = await _userManager.CheckPasswordAsync(user, command.Password);
                    if (!isPasswordValid)
                    {
                        await _userManager.AccessFailedAsync(user);
                        throw new UnAuthorizedException();
                    }
                });
        }
    }
}
