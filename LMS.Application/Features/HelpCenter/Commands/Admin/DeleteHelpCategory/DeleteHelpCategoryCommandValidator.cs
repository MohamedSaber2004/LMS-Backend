using FluentValidation;
using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpCategory
{
    public class DeleteHelpCategoryCommandValidator : AbstractValidator<DeleteHelpCategoryCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteHelpCategoryCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            DeleteHelpCategoryCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            if (command.Id == Guid.Empty)
            {
                return;
            }

            var categoryExists = await _context.HelpCategories
                .AnyAsync(category => category.Id == command.Id, cancellationToken);

            // A missing category is left to the handler so the caller gets 404, same as the update.
            if (!categoryExists)
            {
                return;
            }

            // The article query filter already hides soft-deleted rows, so only live articles block.
            var categoryInUse = await _context.HelpArticles
                .AnyAsync(article => article.CategoryId == command.Id, cancellationToken);

            if (categoryInUse)
            {
                context.AddFailure(
                    nameof(DeleteHelpCategoryCommand.Id),
                    _localizer[LocalizationKeys.HelpCenter.CategoryInUse].Value);
            }
        }
    }
}
