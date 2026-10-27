using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Attachment.Commands.UploadMultipleAttachments
{
    public class UploadMultipleAttachmentsCommandHandler : IRequestHandler<UploadMultipleAttachmentsCommand, Result<List<string>>>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadMultipleAttachmentsCommandHandler(
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

        public async Task<Result<List<string>>> Handle(UploadMultipleAttachmentsCommand request, CancellationToken cancellationToken)
        {
            if (request.Files is null || request.Files.Count == 0)
                return Result<List<string>>.Failure(_localizer[LocalizationKeys.Attachments.NoMediaProvided].Value,
                    StatusCodes.Status400BadRequest);

            var (uploaded, result) = await UploadAsync(request);

            if (!uploaded)
                return Result<List<string>>.Failure(result, StatusCodes.Status400BadRequest);

            var paths = result
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            return Result<List<string>>.Success(paths);
        }

        private Task<(bool Uploaded, string Result)> UploadAsync(UploadMultipleAttachmentsCommand request)
        {
            return request.MediaType switch
            {
                MediaKind.Image => _imageValidator.UploadMultipleImage(request.Files, request.Place),
                MediaKind.Video => _videoValidator.UploadMultipleVideo(request.Files, request.Place),
                MediaKind.Audio => UploadMultipleAudioAsync(request.Files!, request.Place),
                MediaKind.Document => _fileValidator.UploadMultipleFile(request.Files, request.Place),
                _ => Task.FromResult((false, _localizer[LocalizationKeys.Attachments.UnsupportedMediaKind].Value))
            };
        }

        private async Task<(bool Uploaded, string Result)> UploadMultipleAudioAsync(List<IFormFile> files, int place)
        {
            var uploadedNames = new List<string>();

            foreach (var file in files)
            {
                var (uploaded, path) = await _audioValidator.UploadAudio(file, place);
                if (uploaded)
                    uploadedNames.Add(path);
            }

            return uploadedNames.Count == 0
                ? (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value)
                : (true, string.Join(",", uploadedNames));
        }
    }
}
