using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Features.Authentication.Queries.GetUserProfile
{
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        public GetUserProfileQueryValidator(
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x)
                .CustomAsync(async (_, _, _) =>
                {
                    if (!currentUserService.IsAuthenticated || currentUserService.UserId == Guid.Empty)
                    {
                        throw new UnAuthorizedException();
                    }

                    var user = await userManager.FindByIdAsync(currentUserService.UserId.ToString());
                    if (user is null || user.IsDeleted || !user.IsActive)
                    {
                        throw new UnAuthorizedException();
                    }
                });
        }
    }
}
