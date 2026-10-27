using LMS.Application.Common.Models;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Attachment.Commands.UploadMultipleAttachments
{
    public class UploadMultipleAttachmentsCommand: IRequest<Result<List<string>>>
    {
        public List<IFormFile>? Files { get; set; }
        public MediaKind MediaType { get; set; }
        public int Place { get; set; }
    }
}
