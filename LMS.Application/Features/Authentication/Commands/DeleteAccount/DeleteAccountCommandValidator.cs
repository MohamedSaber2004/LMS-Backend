using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.DeleteAccount
{
    public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountCommandValidator(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.PasswordRequired].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(command.Password))
                    {
                        return;
                    }

                    var user = await userManager.FindByIdAsync(currentUserService.UserId.ToString());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }

                    if (!await userManager.CheckPasswordAsync(user, command.Password))
                    {
                        context.AddFailure(
                            nameof(command.Password),
                            localizer[LocalizationKeys.Authentication.CurrentPasswordIncorrect].Value);
                    }
                });
        }
    }
}
