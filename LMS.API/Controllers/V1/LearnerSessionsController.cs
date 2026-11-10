using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.RateSession;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Learner]
    /// Learner-only session action that no other controller exposes. Listing, booking, cancelling and
    /// attendance confirmation for a learner all live under /api/v1/sessions.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Learner.Sessions.Base)]
    [Tags("Learner: Sessions")]
    [Authorize]
    [RoleAuthorize(UserType.Learner)]
    public class LearnerSessionsController : BaseController
    {
        /// <summary>
        /// Submit a 1-5 star rating and feedback review for a completed session.
        /// </summary>
        [HttpPost(ApiRoutes.Learner.Sessions.Rate)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Rate(
            [FromRoute] Guid id,
            [FromBody] RateSessionCommand command,
            CancellationToken cancellationToken)
        {
            command.SessionId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
