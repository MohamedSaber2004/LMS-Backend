using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Features.Availability.Queries.CheckVacationConflicts;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// Availability management and vacation conflict verification for instructors.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Availability.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherAvailabilityController : BaseController
    {
        /// <summary>
        /// Check whether the instructor has any confirmed or pending sessions that conflict with a proposed vacation window.
        /// </summary>
        [HttpPost(ApiRoutes.Teacher.Availability.CheckVacationConflicts)]
        [ProducesResponseType(typeof(ApiResponse<VacationConflictResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CheckVacationConflicts(
            [FromBody] CheckVacationConflictsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}
