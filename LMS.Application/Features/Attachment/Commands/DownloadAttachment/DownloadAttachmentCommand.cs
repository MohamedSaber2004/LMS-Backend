using LMS.Application.Common.Models;
using LMS.Application.Features.Attachment.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Attachment.Commands.DownloadAttachment
{
    public class DownloadAttachmentCommand: IRequest<Result<AttachmentDownloadDto>>
    {
        public string? FileName { get; set; }
        public int FilePlace { get; set; }
        public MediaKind MediaType { get; set; }
    }
}
