using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpCategory
{
    public class AddHelpCategoryCommandValidator : AbstractValidator<AddHelpCategoryCommand>
    {
        private const int NameMaxLength = 150;

        private readonly ILmsDbContext _context;

        public AddHelpCategoryCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameRequired].Value)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameRequired].Value)
                .MaximumLength(NameMaxLength)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameTooLong, NameMaxLength].Value)
                .MustAsync((name, cancellationToken) =>
                    HelpCenterValidation.CategoryNameIsAvailableAsync(
                        _context, name, excludedCategoryId: null, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNameAlreadyExists].Value);

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.OrderMustBeNonNegative].Value);
        }
    }
}
