using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Domain.Enums;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdQueryValidator : AbstractValidator<GetSessionByIdQuery>
    {
        private readonly ILmsDbContext _context;

        public GetSessionByIdQueryValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(query => query.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SessionIdRequired].Value);

            RuleFor(query => query)
                .MustAsync(async (query, cancellationToken) =>
                {
                    if (ActorOwnership.HasAdminBoardAccess(currentUser, AdminPermission.SessionsRead))
                    {
                        return true;
                    }

                    var userId = currentUser.UserId;

                    var allowed = await SessionsValidation.InstructorOwnsSessionAsync(
                            _context, query.Id, userId, cancellationToken)
                        || await SessionsValidation.LearnerHoldsBookingOnSessionAsync(
                            _context, query.Id, userId, cancellationToken);

                    if (!allowed)
                    {
                        throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
                    }

                    return true;
                })
                .When(query => query.Id != Guid.Empty);
        }
    }
}
