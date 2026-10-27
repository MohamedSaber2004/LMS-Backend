using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Attachment.Commands.DownloadAttachment;
using LMS.Application.Features.Attachment.Commands.UpdateAttachment;
using LMS.Application.Features.Attachment.Commands.UploadAttachment;
using LMS.Application.Features.Attachment.Commands.UploadMultipleAttachments;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Attachments.Base)]
    //[RoleAuthorize]
    public class AttachmentsController : BaseController
    {
        /// <summary>
        /// Upload an attachment file to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Attachments.Upload)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Upload([FromForm] UploadAttachmentCommand command, CancellationToken cancellationToken)
        {
            return FromResult(await Mediator.Send(command, cancellationToken));
        }

        /// <summary>
        /// Upload multiple attachment files to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Attachments.UploadMultiple)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadMultiple([FromForm] UploadMultipleAttachmentsCommand command, CancellationToken cancellationToken)
        {
            return FromResult(await Mediator.Send(command, cancellationToken));
        }


        /// <summary>
        /// Update an existing attachment file on the server.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Attachments.Update)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Update([FromForm] UpdateAttachmentCommand command, CancellationToken cancellationToken)
        {
            return FromResult(await Mediator.Send(command, cancellationToken));
        }

        /// <summary>
        /// Download an attachment file from the server.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Attachments.Download)]
        [Produces("application/octet-stream", "application/json")]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Download([FromQuery] DownloadAttachmentCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);

            if (!result.IsSuccess || result.Data is null)
                return FromResult(result);

            var attachment = result.Data;
            return File(attachment.Content, attachment.ContentType, attachment.FileName);
        }
    }
}
