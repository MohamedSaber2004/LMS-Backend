using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.Commands.Admin.AddAvailabilitySlot;
using LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot;
using LMS.Application.Features.Availability.Commands.Admin.EditAvailabilitySlot;
using LMS.Application.Features.Availability.Commands.SetAvailabilityDay;
using LMS.Application.Features.Availability.Commands.SetAvailabilityWindow;
using LMS.Application.Features.Availability.Commands.SetVacationMode;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Features.Availability.Queries.GetMyAvailability;
using LMS.Application.Features.Availability.Queries.GetMyAvailabilityCalendar;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// Availability self-service for the signed-in instructor. Every write is pinned to the caller's
    /// own instructor row, so no instructor id is ever taken from the request body or route here.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.InstructorAvailability.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class MyAvailabilityController : BaseController
    {
        /// <summary>
        /// Get the signed-in instructor's weekly slots, derived time zone and vacation mode.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorAvailability.My)]
        [ProducesResponseType(typeof(ApiResponse<MyAvailabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMyAvailabilityQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Add a recurring weekly availability slot for the signed-in instructor.
        /// Times are the instructor's own local wall-clock values.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.InstructorAvailability.MySlots)]
        [ProducesResponseType(typeof(ApiResponse<AvailabilitySlotDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddSlot(
            [FromBody] AddAvailabilitySlotCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update one of the signed-in instructor's weekly slots.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.InstructorAvailability.MySlotById)]
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
        /// Soft delete one of the signed-in instructor's weekly slots.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.InstructorAvailability.MySlotById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSlot([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteAvailabilitySlotCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Turn vacation mode on or off for the signed-in instructor. While it is on, no new booking
        /// can be placed; already confirmed sessions keep their slots.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.InstructorAvailability.MyVacationMode)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetVacationMode(
            [FromBody] SetVacationModeCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get the dated availability of the signed-in instructor for a local date range: which days
        /// are open, blocked or already booked, with the exact UTC instants of every window.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.InstructorAvailability.MyCalendar)]
        [ProducesResponseType(typeof(ApiResponse<AvailabilityCalendarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyCalendar(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(
                new GetMyAvailabilityCalendarQuery { From = from, To = to },
                cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Open or block one dated day of the signed-in instructor's calendar.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.InstructorAvailability.MyCalendarDay)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetDay(
            [FromBody] SetAvailabilityDayCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Open or block one dated availability window of the signed-in instructor, leaving the rest of
        /// that day untouched.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.InstructorAvailability.MyCalendarWindow)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetWindow(
            [FromBody] SetAvailabilityWindowCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
