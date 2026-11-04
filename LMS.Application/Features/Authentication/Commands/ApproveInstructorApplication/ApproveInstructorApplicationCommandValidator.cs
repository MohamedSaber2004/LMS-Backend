using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication
{
    public class ApproveInstructorApplicationCommandValidator : AbstractValidator<ApproveInstructorApplicationCommand>
    {
        private const int ReviewNotesMaxLength = 1000;

        public ApproveInstructorApplicationCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ApplicationId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Authentication.ApplicationIdRequired].Value);

            RuleFor(x => x.ReviewNotes)
                .MaximumLength(ReviewNotesMaxLength)
                .WithMessage(localizer[LocalizationKeys.Authentication.ReviewNotesTooLong, ReviewNotesMaxLength].Value)
                .When(x => !string.IsNullOrWhiteSpace(x.ReviewNotes));

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.ApplicationId == Guid.Empty)
                    {
                        return;
                    }

                    var application = await context.InstructorApplications
                        .AsNoTracking()
                        .Where(item => item.Id == command.ApplicationId)
                        .Select(item => new
                        {
                            item.Status,
                            // Nullable: a soft-deleted instructor is filtered out of the join.
                            EmailConfirmed = (bool?)item.Instructor!.User.EmailConfirmed
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (application is null)
                    {
                        validationContext.AddFailure(
                            nameof(command.ApplicationId),
                            localizer[LocalizationKeys.Authentication.InstructorApplicationNotFound].Value);
                        return;
                    }

                    if (application.Status != InstructorApplicationStatus.Pending)
                    {
                        validationContext.AddFailure(
                            nameof(command.ApplicationId),
                            localizer[LocalizationKeys.Authentication.InstructorApplicationAlreadyReviewed].Value);
                        return;
                    }

                    // Approval force-marks the email as confirmed, so an unreachable address would be
                    // blessed silently. The verification code is sent at registration but never gates it.
                    if (application.EmailConfirmed != true)
                    {
                        validationContext.AddFailure(
                            nameof(command.ApplicationId),
                            localizer[LocalizationKeys.Authentication.InstructorEmailNotVerified].Value);
                    }
                });
        }
    }
}
