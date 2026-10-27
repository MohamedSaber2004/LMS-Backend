using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Attachment.Commands.UploadAttachment
{
    public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, Result<string>>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadAttachmentCommandHandler(
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

        public async Task<Result<string>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return Result<string>.Failure(_localizer[LocalizationKeys.Attachments.FileRequired].Value,
                    StatusCodes.Status400BadRequest);

            var (uploaded, result) = await UploadAsync(request);

            return uploaded
                ? Result<string>.Success(result)
                : Result<string>.Failure(result, StatusCodes.Status400BadRequest);
        }

        private Task<(bool Uploaded, string Result)> UploadAsync(UploadAttachmentCommand request)
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
    }
}
