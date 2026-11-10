using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.AdminCancelSession;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetAdminSessions;
using LMS.Application.Features.Sessions.Queries.GetSessionById;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin]
    /// Admin management and monitoring of all platform live and scheduled sessions, including force cancellations.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Sessions.Base)]
    [Tags("Admin: Sessions")]
    [Authorize]
    [RoleAuthorize(UserType.Admin)]
    public class AdminSessionsController : BaseController
    {
        /// <summary>
        /// List all platform sessions with status, teacher, learner, trial, and date filters.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Sessions.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<AdminSessionListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAdminSessionsQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get single session details and booking information.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Sessions.GetById)]
        [ProducesResponseType(typeof(ApiResponse<SessionDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSessionByIdQuery { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Force cancel a session administratively (refunds package hour to student if regular session).
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Sessions.Cancel)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] AdminCancelSessionCommand command,
            CancellationToken cancellationToken)
        {
            command.SessionId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
