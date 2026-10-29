using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetUserByIdQueryValidator(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Users.UserIdRequired].Value);

            RuleFor(x => x)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            GetUserByIdQuery query,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (query.Id == Guid.Empty)
            {
                return;
            }

            var userExists = await _context.ApplicationUsers
                .AnyAsync(user => user.Id == query.Id && !user.IsDeleted, cancellationToken);

            if (!userExists)
            {
                context.AddFailure(
                    nameof(GetUserByIdQuery.Id),
                    _localizer[LocalizationKeys.Users.UserNotFound].Value);
            }
        }
    }
}
