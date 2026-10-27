using LMS.Application.Common.Models;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Attachment.Commands.UploadAttachment
{
    public class UploadAttachmentCommand: IRequest<Result<string>>
    {
        public IFormFile? File { get; set; }
        public MediaKind MediaType { get; set; }
        public int Place { get; set; }
    }
}
