using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.WithdrawReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor (Teacher)]
    /// Reschedule requests queries and withdrawal operations for instructors.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.RescheduleRequests.Base)]
    [Tags("Teacher: Reschedule Requests")]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherRescheduleRequestsController : BaseController
    {
        /// <summary>
        /// List reschedule requests for sessions taught by the caller with optional status filter (defaults to pending).
        /// </summary>
        [HttpGet(ApiRoutes.Teacher.RescheduleRequests.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<TeacherRescheduleRequestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetTeacherRescheduleRequestsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Withdraw a pending reschedule request proposed by the instructor before learner approval.
        /// </summary>
        [HttpPost(ApiRoutes.Teacher.RescheduleRequests.Withdraw)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Withdraw(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new WithdrawRescheduleRequestCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}

