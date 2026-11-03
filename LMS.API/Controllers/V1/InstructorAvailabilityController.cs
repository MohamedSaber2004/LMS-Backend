using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityForLearner;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorAvailability.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Learner)]
    public class InstructorAvailabilityController : BaseController
    {
        /// <summary>
        /// Get the weekly availability of a selected instructor.
        /// Times are the instructor's local wall-clock values; resolve the timezone from the
        /// returned country before showing them to the learner.
        /// </summary>
        /// <param name="instructorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorAvailability.ForInstructor)]
        [ProducesResponseType(typeof(ApiResponse<InstructorAvailabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForInstructor(
            [FromRoute] Guid instructorId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(
                new GetInstructorAvailabilityForLearnerQuery { InstructorId = instructorId },
                cancellationToken);
            return FromResult(result);
        }
    }
}
