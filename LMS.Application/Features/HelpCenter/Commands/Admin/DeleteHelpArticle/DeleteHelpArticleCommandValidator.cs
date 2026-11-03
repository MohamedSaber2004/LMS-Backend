using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpArticle
{
    public class DeleteHelpArticleCommandValidator : AbstractValidator<DeleteHelpArticleCommand>
    {
        public DeleteHelpArticleCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.ArticleIdRequired].Value);
        }
    }
}
