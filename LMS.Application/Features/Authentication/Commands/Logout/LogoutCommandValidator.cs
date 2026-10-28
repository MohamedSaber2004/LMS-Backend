using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;

namespace LMS.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator(ICurrentUserService currentUserService)
        {
            RuleFor(x => x)
                .Custom((_, _) =>
                {
                    if (!currentUserService.IsAuthenticated || currentUserService.UserId == Guid.Empty)
                    {
                        throw new UnAuthorizedException();
                    }
                });
        }
    }
}
