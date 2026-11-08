using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.Commands.SetTeacherVacationMode;
using LMS.Application.Features.Availability.DTOs;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// Availability management and automated vacation mode handling for instructors.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Availability.Base)]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherAvailabilityController : BaseController
    {
        /// <summary>
        /// Enable or disable vacation mode for the instructor.
        /// Automatically checks for conflicting sessions, returns a conflict flag/summary, and triggers background processing via Hangfire.
        /// </summary>
        [HttpPut(ApiRoutes.Teacher.Availability.VacationMode)]
        [ProducesResponseType(typeof(ApiResponse<TeacherVacationModeResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetVacationMode(
            [FromBody] SetTeacherVacationModeCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
