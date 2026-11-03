using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.Commands.Admin.AddAvailabilitySlot;
using LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot;
using LMS.Application.Features.Availability.Commands.Admin.EditAvailabilitySlot;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityPagginated;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorAvailability.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Admin)]
    public class InstructorAvailabilityAdminController : BaseController
    {
        /// <summary>
        /// Get availability slots with pagination, optionally filtered by instructor and day.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorAvailability.GetAllPagginated)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<InstructorAvailabilityRowDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllPagginated(
            [FromQuery] GetInstructorAvailabilityPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Add a recurring weekly availability slot for an instructor.
        /// Times are stored as the instructor's own local wall-clock values.
        /// </summary>
        /// <param name="instructorId"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.InstructorAvailability.Slots)]
        [ProducesResponseType(typeof(ApiResponse<AvailabilitySlotDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddSlot(
            [FromRoute] Guid instructorId,
            [FromBody] AddAvailabilitySlotCommand command,
            CancellationToken cancellationToken)
        {
            command.InstructorId = instructorId;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update an availability slot. The slot stays on the same instructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.InstructorAvailability.SlotById)]
        [ProducesResponseType(typeof(ApiResponse<AvailabilitySlotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditSlot(
            [FromRoute] Guid id,
            [FromBody] EditAvailabilitySlotCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete an availability slot.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.InstructorAvailability.SlotById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSlot([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteAvailabilitySlotCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
