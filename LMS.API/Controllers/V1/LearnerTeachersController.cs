using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Features.Onboarding.Queries.GetInstructorDetailsForLearner;
using LMS.Application.Features.Onboarding.Queries.GetInstructorsForLearner;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetBookableSlots;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Learner / Public Guest]
    /// Student &amp; Landing Page API for searching teachers, viewing teacher public profiles, and exploring availability schedules with timezone sync.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Learner.Teachers.Base)]
    [Tags("Learner: Teachers")]
    public class LearnerTeachersController : BaseController
    {
        /// <summary>
        /// Search and filter teachers for landing page and mobile app (by language, native speaker status, rating ranking Top 5/10, price range, etc.).
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Teachers.GetAll)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<InstructorCardDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetInstructorsForLearnerQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// View detailed public profile of a teacher for landing page and mobile app.
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Teachers.GetById)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<InstructorCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetInstructorDetailsForLearnerQuery { InstructorId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// View teacher availability schedule and slots, automatically converted to the student's local timezone.
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Teachers.Schedule)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BookableSlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSchedule(
            [FromRoute] Guid id,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] string? timeZoneId,
            CancellationToken cancellationToken)
        {
            var query = new GetBookableSlotsQuery
            {
                InstructorId = id,
                From = from,
                To = to,
                TimeZoneId = timeZoneId
            };
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}

