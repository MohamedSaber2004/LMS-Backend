using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpArticle
{
    public class UpdateHelpArticleCommandValidator : AbstractValidator<UpdateHelpArticleCommand>
    {
        private const int QuestionMaxLength = 300;
        private const int AnswerMaxLength = 20000;

        public UpdateHelpArticleCommandValidator(IStringLocalizer<Messages> localizer)
        {
            // A missing article is reported as 404 by the handler.
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.ArticleIdRequired].Value);

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
