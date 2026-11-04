using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.ApproveReschedule;
using LMS.Application.Features.Sessions.Commands.DeclineReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.RescheduleRequests.Base)]
    [RoleAuthorize(UserType.Instructor, UserType.Learner)]
    public class RescheduleRequestsController : BaseController
    {
        /// <summary>
        /// Accept the proposed window and move the session onto it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.RescheduleRequests.Approve)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ApproveRescheduleCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Refuse the proposed window and keep the session at its current time.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.RescheduleRequests.Decline)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Decline([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeclineRescheduleCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
