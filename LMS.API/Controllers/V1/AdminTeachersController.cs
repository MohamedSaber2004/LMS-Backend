using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.Commands.ToggleInstructorStatus;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Features.InstructorProfile.Queries.GetAdminTeachers;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Features.Wallet.Queries.GetAdminTeacherWallet;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin]
    /// Admin directory of approved teachers: search and filter the roster, toggle a teacher active, and read a teacher wallet balance.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Teachers.Base)]
    [Tags("Admin: Teachers & Pricing")]
    [Authorize]
    [RoleAuthorize(UserType.Admin)]
    public class AdminTeachersController : BaseController
    {
        /// <summary>
        /// List approved instructors on the platform with search and status filters.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Teachers.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<AdminTeacherListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAdminTeachersQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Activate or deactivate a teacher's account.
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Teachers.ToggleStatus)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ToggleInstructorStatusCommand { InstructorId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// View teacher wallet balance, pending balance, and total earnings.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Teachers.Wallet)]
        [ProducesResponseType(typeof(ApiResponse<TeacherWalletSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacherWallet([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAdminTeacherWalletQuery { InstructorId = id }, cancellationToken);
            return FromResult(result);
        }

    }
}

