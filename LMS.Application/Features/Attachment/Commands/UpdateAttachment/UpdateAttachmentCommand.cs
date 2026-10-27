using LMS.Application.Common.Models;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace LMS.Application.Features.Attachment.Commands.UpdateAttachment
{
    public class UpdateAttachmentCommand: IRequest<Result<string>>
    {
        public string? OldFileName { get; set; }
        public IFormFile? File { get; set; }
        public MediaKind MediaType { get; set; }
        public int Place { get; set; }
    }
}
