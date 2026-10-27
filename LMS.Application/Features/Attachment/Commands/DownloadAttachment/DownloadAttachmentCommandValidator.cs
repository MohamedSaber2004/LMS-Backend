using FluentValidation;
using LMS.Application.Common.Services;
using LMS.Application.Localization;

namespace LMS.Application.Features.Attachment.Commands.DownloadAttachment
{
    public class DownloadAttachmentCommandValidator : AbstractValidator<DownloadAttachmentCommand>
    {
        public DownloadAttachmentCommandValidator()
        {
            RuleFor(x => x.FileName)
                .NotNull()
                .NotEmpty()
                .WithMessage(LocalizationKeys.Attachments.StoredNameRequired);

            RuleFor(x => x.FileName)
                .Must(BeASingleFileName)
                .WithMessage(LocalizationKeys.ExceptionMessages.BadRequest)
                .When(x => !string.IsNullOrWhiteSpace(x.FileName));

            RuleFor(x => x.MediaType)
                .IsInEnum()
                .WithMessage(LocalizationKeys.Attachments.UnsupportedMediaKind);

            RuleFor(x => x)
                .Must(HasConfiguredPlace)
                .WithMessage(LocalizationKeys.Attachments.InvalidPlace);
        }

        private static bool BeASingleFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return Path.GetFileName(fileName.Trim()) == fileName.Trim();
        }

        private static bool HasConfiguredPlace(DownloadAttachmentCommand command)
        {
            return !string.IsNullOrWhiteSpace(UploadPaths.GetPath(command.FilePlace))
                || !string.IsNullOrWhiteSpace(UploadPaths.GetPath((int)command.MediaType));
        }
    }
}
