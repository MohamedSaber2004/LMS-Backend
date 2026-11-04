using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.Commands.ResubmitInstructorApplication;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Features.Authentication.Queries.GetMyInstructorApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorApplications.Base)]
    [RoleAuthorize]
    public class MyInstructorApplicationController : BaseController
    {
        /// <summary>
        /// Read the calling instructor's own application status and review outcome.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorApplications.MyStatus)]
        [ProducesResponseType(typeof(ApiResponse<MyInstructorApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyStatus(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMyInstructorApplicationQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Correct and resubmit a rejected instructor application for admin review.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.InstructorApplications.Resubmit)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Resubmit(
            [FromForm] ResubmitInstructorApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
