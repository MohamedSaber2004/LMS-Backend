using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommandValidator : AbstractValidator<UpdateHelpCategoryCommand>
    {
        private const int NameMaxLength = 150;

        private readonly ILmsDbContext _context;

        public UpdateHelpCategoryCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            // A missing category is reported as 404 by the handler; the validator only checks the
            // request's own shape, matching EditAvailabilitySlotCommandValidator.
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryIdRequired].Value);

            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameRequired].Value)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameRequired].Value)
                .MaximumLength(NameMaxLength)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameTooLong, NameMaxLength].Value);

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) =>
                    HelpCenterValidation.CategoryNameIsAvailableAsync(
                        _context, command.Name, command.Id, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameAlreadyExists].Value)
                .When(command => !string.IsNullOrWhiteSpace(command.Name));

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.OrderMustBeNonNegative].Value);
        }
    }
}
