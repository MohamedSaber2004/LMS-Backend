using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
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

            // Only the teacher of the session, or a learner holding a seat on it, may read it.
            // Throwing keeps the answer a 403 instead of a field error.
            RuleFor(query => query)
                .MustAsync(async (query, cancellationToken) =>
                {
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
