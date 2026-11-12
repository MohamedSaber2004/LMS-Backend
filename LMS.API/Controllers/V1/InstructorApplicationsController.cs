using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication;
using LMS.Application.Features.Authentication.Commands.RejectInstructorApplication;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Features.Authentication.Queries.GetInstructorApplications;
using LMS.Application.Features.Authentication.Queries.GetInstructorApplicationById;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin · SubAdmin (per granted permission code)]
    /// Review instructor applications: list, inspect, approve with initial pricing, reject, and set pricing.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorApplications.Base)]
    [Tags("Admin: Instructor Applications")]
    [RoleAuthorize]
    public class InstructorApplicationsController : BaseController
    {
        /// <summary>
        /// Get instructor applications, filtered by status (pending by default).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [RequirePermission(AdminPermission.InstructorApplicationsRead)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<InstructorApplicationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] GetInstructorApplicationsQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get instructor application details by application ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorApplications.GetById)]
        [RequirePermission(AdminPermission.InstructorApplicationsRead)]
        [ProducesResponseType(typeof(ApiResponse<InstructorApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInstructorApplicationByIdQuery(id), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Approve an instructor application and activate the instructor account.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.InstructorApplications.Approve)]
        [RequirePermission(AdminPermission.InstructorApplicationsUpdate)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve([FromBody] ApproveInstructorApplicationCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Reject a pending instructor application.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.InstructorApplications.Reject)]
        [RequirePermission(AdminPermission.InstructorApplicationsUpdate)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reject([FromBody] RejectInstructorApplicationCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update teacher hourly rate and company commission rate (CCR) by Admin.
        /// </summary>
        [HttpPut(ApiRoutes.InstructorApplications.SetPricing)]
        [RequirePermission(AdminPermission.InstructorPricingUpdate)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPricing(
            [FromRoute] Guid id,
            [FromBody] LMS.Application.Features.InstructorProfile.Commands.SetInstructorPricingAdmin.SetInstructorPricingAdminCommand command,
            CancellationToken cancellationToken)
        {
            command.InstructorId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
