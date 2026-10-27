using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Attachment.Commands.UpdateAttachment
{
    public class UpdateAttachmentCommandHandler : IRequestHandler<UpdateAttachmentCommand, Result<string>>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateAttachmentCommandHandler(
            IImageValidator imageValidator,
            IVideoValidator videoValidator,
            IAudioValidator audioValidator,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _imageValidator = imageValidator;
            _videoValidator = videoValidator;
            _audioValidator = audioValidator;
            _fileValidator = fileValidator;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(UpdateAttachmentCommand request, CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return Result<string>.Failure(_localizer[LocalizationKeys.Attachments.FileRequired].Value,
                    StatusCodes.Status400BadRequest);

            var (uploaded, result) = await UploadAsync(request);

            if (!uploaded)
                return Result<string>.Failure(result, StatusCodes.Status400BadRequest);

            if (!string.IsNullOrWhiteSpace(request.OldFileName))
                await DeleteOldFileAsync(request.MediaType, request.OldFileName, request.Place);

            return Result<string>.Success(result, _localizer[LocalizationKeys.ActionResults.Updated].Value);
        }

        private Task<(bool Uploaded, string Result)> UploadAsync(UpdateAttachmentCommand request)
        {
            return request.MediaType switch
            {
                MediaKind.Image => _imageValidator.UploadImage(request.File, request.Place),
                MediaKind.Video => _videoValidator.UploadVideo(request.File, request.Place),
                MediaKind.Audio => _audioValidator.UploadAudio(request.File, request.Place),
                MediaKind.Document => _fileValidator.UploadFile(request.File, request.Place),
                _ => Task.FromResult((false, _localizer[LocalizationKeys.Attachments.UnsupportedMediaKind].Value))
            };
        }

        private Task<bool> DeleteOldFileAsync(MediaKind mediaType, string oldFileName, int place)
        {
            return mediaType switch
            {
                MediaKind.Image => _imageValidator.DeleteImage(oldFileName, place),
                MediaKind.Video => _videoValidator.DeleteVideo(oldFileName, place),
                MediaKind.Audio => _audioValidator.DeleteAudio(oldFileName, place),
                MediaKind.Document => _fileValidator.DeleteFile(oldFileName, place),
                _ => Task.FromResult(false)
            };
        }
    }
}
