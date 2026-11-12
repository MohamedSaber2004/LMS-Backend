using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.Commands.UpdateInstructorProfile;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Features.InstructorProfile.Queries.GetInstructorProfile;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor (Teacher)]
    /// Teacher-managed public bio, specialization, years of experience, and credentials management.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Profile.Base)]
    [Tags("Teacher: Profile")]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class InstructorProfileController : BaseController
    {
        /// <summary>
        /// Read the calling instructor's own profile: instructor fields plus the base identity profile.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InstructorProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInstructorProfileQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update the instructor-managed profile fields, optionally together with the base identity fields.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<InstructorProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateInstructorProfileCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
