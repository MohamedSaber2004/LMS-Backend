using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Attachment.Commands.UploadMultipleAttachments
{
    public class UploadMultipleAttachmentsCommandValidator: AbstractValidator<UploadMultipleAttachmentsCommand>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IFileValidator _fileValidator;

        public UploadMultipleAttachmentsCommandValidator(
            IImageValidator imageValidator,
            IVideoValidator videoValidator,
            IAudioValidator audioValidator,
            IFileValidator fileValidator)
        {
            _imageValidator = imageValidator;
            _videoValidator = videoValidator;
            _audioValidator = audioValidator;
            _fileValidator = fileValidator;

            RuleFor(x => x.Files)
                .NotNull()
                .NotEmpty()
                .WithMessage(LocalizationKeys.Attachments.NoMediaProvided);

            RuleFor(x => x.Place)
                .GreaterThanOrEqualTo(0)
                .Must(place => !string.IsNullOrWhiteSpace(UploadPaths.GetPath(place)))
                .WithMessage(LocalizationKeys.ExceptionMessages.BadRequest);

            RuleForEach(x => x.Files)
                .Must((command, file) => IsValidForMediaType(command.MediaType, file))
                .WithMessage(command => GetInvalidFormatKey(command.MediaType))
                .When(x => x.Files != null);
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

        private static string GetInvalidFormatKey(MediaKind mediaType)
        {
            return mediaType switch
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
