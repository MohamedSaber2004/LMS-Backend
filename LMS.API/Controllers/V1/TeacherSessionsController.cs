using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.CancelSession;
using LMS.Application.Features.Sessions.Commands.RequestReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetSessionParticipants;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor (Teacher)]
    /// Teacher-side session actions that learners cannot perform: cancelling a session, answering with a reschedule request, and reading the participants roster.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Sessions.Base)]
    [Tags("Teacher: Sessions")]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherSessionsController : BaseController
    {
        /// <summary>
        /// Teacher cancels a session (enforces > 24h cutoff rule TBR-007).
        /// </summary>
        [HttpPost(ApiRoutes.Teacher.Sessions.Cancel)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelSessionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Propose an alternative window for the session (enforces > 24h cutoff TBR-007 and single pending TBR-010).
        /// </summary>
        [HttpPost(ApiRoutes.Teacher.Sessions.RescheduleRequests)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestReschedule(
            [FromRoute] Guid id,
            [FromBody] RequestRescheduleCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get the participants list and attendance records for a session taught by the calling instructor.
        /// </summary>
        [HttpGet(ApiRoutes.Teacher.Sessions.Participants)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SessionParticipantDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetParticipants(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSessionParticipantsQuery { SessionId = id }, cancellationToken);
            return FromResult(result);
        }
    }
}

