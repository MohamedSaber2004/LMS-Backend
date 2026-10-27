using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Services;
using LMS.Application.Features.Attachment.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Attachment.Commands.DownloadAttachment
{
    public class DownloadAttachmentCommandHandler : IRequestHandler<DownloadAttachmentCommand, Result<AttachmentDownloadDto>>
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DownloadAttachmentCommandHandler(
            IBaseFileService baseFileService,
            IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<Result<AttachmentDownloadDto>> Handle(DownloadAttachmentCommand request, CancellationToken cancellationToken)
        {
            var (place, fileName) = ResolveStoredName(request);

            var folderPath = UploadPaths.GetPath(place);
            if (string.IsNullOrWhiteSpace(folderPath))
                return Result<AttachmentDownloadDto>.Failure(_localizer[LocalizationKeys.Attachments.InvalidPlace].Value,
                    StatusCodes.Status400BadRequest);

            var (found, physicalPath) = await _baseFileService.DownloadFileAsync(folderPath, fileName);
            if (!found)
                return Result<AttachmentDownloadDto>.Failure(_localizer[LocalizationKeys.Attachments.FileNotFound].Value,
                    StatusCodes.Status404NotFound);

            var content = new FileStream(physicalPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.SequentialScan);

            var contentType = ContentTypeProvider.TryGetContentType(fileName, out var resolved)
                ? resolved
                : "application/octet-stream";

            return Result<AttachmentDownloadDto>.Success(
                new AttachmentDownloadDto(content, fileName, contentType, content.Length));
        }

        private static (int Place, string FileName) ResolveStoredName(DownloadAttachmentCommand request)
        {
            var fileName = request.FileName?.Trim() ?? string.Empty;

            var separatorIndex = fileName.IndexOf('_');
            if (separatorIndex > 0
                && int.TryParse(fileName.AsSpan(0, separatorIndex), out var prefixedPlace))
            {
                return (prefixedPlace, fileName[(separatorIndex + 1)..]);
            }

            if (!string.IsNullOrWhiteSpace(UploadPaths.GetPath(request.FilePlace)))
                return (request.FilePlace, fileName);

            return ((int)request.MediaType, fileName);
        }
    }
}
