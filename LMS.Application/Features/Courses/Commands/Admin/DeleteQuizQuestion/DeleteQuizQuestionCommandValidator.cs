using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteQuizQuestion
{
    public class DeleteQuizQuestionCommandValidator : AbstractValidator<DeleteQuizQuestionCommand>
    {
        public DeleteQuizQuestionCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionIdRequired].Value)
                .MustAsync((questionId, cancellationToken) =>
                    CoursesValidation.QuestionExistsAsync(context, questionId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Courses.QuestionNotFound].Value);
        }
    }
}
