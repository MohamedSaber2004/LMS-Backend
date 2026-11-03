using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticleById
{
    public class GetActiveHelpArticleByIdQueryValidator
        : AbstractValidator<GetActiveHelpArticleByIdQuery>
    {
        public GetActiveHelpArticleByIdQueryValidator(IStringLocalizer<Messages> localizer)
        {
            // A missing or unpublished article is reported as 404 by the handler.
            RuleFor(query => query.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.HelpCenter.ArticleIdRequired].Value);
        }
    }
}
