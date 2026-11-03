using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpArticle
{
    public class AddHelpArticleCommandValidator : AbstractValidator<AddHelpArticleCommand>
    {
        private const int QuestionMaxLength = 300;
        private const int AnswerMaxLength = 20000;

        private readonly ILmsDbContext _context;

        public AddHelpArticleCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.CategoryId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryIdRequired].Value)
                .MustAsync((categoryId, cancellationToken) =>
                    HelpCenterValidation.CategoryExistsAsync(_context, categoryId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.CategoryNotFound].Value);

            RuleFor(command => command.Question)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.QuestionRequired].Value)
                .Must(question => !string.IsNullOrWhiteSpace(question))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.QuestionRequired].Value)
                .MaximumLength(QuestionMaxLength)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.QuestionTooLong, QuestionMaxLength].Value);

            RuleFor(command => command.Answer)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.AnswerRequired].Value)
                .Must(answer => !string.IsNullOrWhiteSpace(answer))
                .WithMessage(localizer[LocalizationKeys.HelpCenter.AnswerRequired].Value)
                .MaximumLength(AnswerMaxLength)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.AnswerTooLong, AnswerMaxLength].Value);

            RuleFor(command => command.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.HelpCenter.OrderMustBeNonNegative].Value);
        }
    }
}
