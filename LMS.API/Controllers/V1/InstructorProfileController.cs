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
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorProfile.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class InstructorProfileController : BaseController
    {
        /// <summary>
        /// Read the calling instructor's own profile, including the live hourly rate.
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
        /// Update the instructor-managed profile fields and the live 1-to-1 rate.
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
