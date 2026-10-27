using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Attachment.Commands.UpdateAttachment
{
    public class UpdateAttachmentCommandValidator: AbstractValidator<UpdateAttachmentCommand>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IFileValidator _fileValidator;

        public UpdateAttachmentCommandValidator(
            IImageValidator imageValidator,
            IVideoValidator videoValidator,
            IAudioValidator audioValidator,
            IFileValidator fileValidator)
        {
            _imageValidator = imageValidator;
            _videoValidator = videoValidator;
            _audioValidator = audioValidator;
            _fileValidator = fileValidator;


            RuleFor(x => x.OldFileName)
                .NotEmpty()
                .WithMessage(LocalizationKeys.Attachments.FileNotFound);

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage(LocalizationKeys.Attachments.FileEmpty);

            RuleFor(x => x.Place)
                .GreaterThanOrEqualTo(0)
                .Must(place => !string.IsNullOrWhiteSpace(UploadPaths.GetPath(place)))
                .WithMessage(LocalizationKeys.ExceptionMessages.BadRequest);

            RuleFor(x => x)
                .Must(IsValidForMediaType)
                .WithMessage(GetInvalidFormatKey)
                .When(x => x.File != null);
        }

        private bool IsValidForMediaType(UpdateAttachmentCommand command)
        {
            return IsValidForMediaType(command.MediaType, command.File);
        }

        private bool IsValidForMediaType(MediaKind mediaType, IFormFile? file)
        {
            return mediaType switch
            {
                MediaKind.Image => _imageValidator.IsValidImage(file),
                MediaKind.Video => _videoValidator.IsValidVideo(file),
                MediaKind.Audio => _audioValidator.IsValidAudio(file),
                MediaKind.Document => _fileValidator.IsValidFile(file),
                _ => false
            };
        }

        private static string GetInvalidFormatKey(UpdateAttachmentCommand command)
        {
            return command.MediaType switch
            {
                MediaKind.Image => LocalizationKeys.Attachments.InvalidImageFormat,
                MediaKind.Video => LocalizationKeys.Attachments.InvalidVideoFormat,
                MediaKind.Audio => LocalizationKeys.Attachments.InvalidAudioFormat,
                MediaKind.Document => LocalizationKeys.Attachments.InvalidFileFormat,
                _ => LocalizationKeys.Attachments.InvalidFormat
            };
        }
    }
}
