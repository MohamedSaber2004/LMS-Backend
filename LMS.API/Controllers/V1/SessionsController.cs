using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.ApproveReschedule;
using LMS.Application.Features.Sessions.Commands.CancelSession;
using LMS.Application.Features.Sessions.Commands.ConfirmSession;
using LMS.Application.Features.Sessions.Commands.CreateBooking;
using LMS.Application.Features.Sessions.Commands.DeclineSession;
using LMS.Application.Features.Sessions.Commands.PaySession;
using LMS.Application.Features.Sessions.Commands.RequestReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetBookableSlots;
using LMS.Application.Features.Sessions.Queries.GetMyBookings;
using LMS.Application.Features.Sessions.Queries.GetMySessions;
using LMS.Application.Features.Sessions.Queries.GetSessionById;
using LMS.Application.Features.Sessions.Queries.GetSessionParticipants;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor and Learner]
    /// Sessions and bookings. The caller is never asked who they are: an instructor gets their own
    /// teaching list, a learner gets the sessions they hold a seat on, and every write re-checks that
    /// the caller really is the teacher or one of the learners of that session.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Sessions.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor, UserType.Learner)]
    public class SessionsController : BaseController
    {
        /// <summary>
        /// List the signed-in user's sessions (for an instructor: teaching sessions; for a learner: booked sessions).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Sessions.GetAllPagginated)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<SessionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMine(
            [FromQuery] GetMySessionsQuery query,
            CancellationToken cancellationToken)
        {
            if (User.IsInRole(UserType.Learner.ToString()) && !User.IsInRole(UserType.Instructor.ToString()))
            {
                var learnerResult = await Mediator.Send(new GetMyBookingsQuery
                {
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    Tab = query.Tab
                }, cancellationToken);
                return FromResult(learnerResult);
            }

            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// List the signed-in learner's bookings.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Sessions.MyBookingsPagginated)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<SessionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] GetMyBookingsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// The dated windows of one instructor that a learner can still book. Dates are the instructor's
        /// own local days.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Sessions.BookableSlots)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BookableSlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBookableSlots(
            [FromQuery] GetBookableSlotsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Student] Book a session with chosen instructor and time slot (deducts 1.0 session for 50m or 0.5 session for 25m from student's active plan wallet; supports free trial).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.GetAllPagginated)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Book(
            [FromBody] CreateBookingCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get one session with its roster and its reschedule requests.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Sessions.GetById)]
        [ProducesResponseType(typeof(ApiResponse<SessionDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSessionByIdQuery { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Student] Complete payment/package deduction for a confirmed session before it starts.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.Pay)]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Pay([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new PaySessionCommand { SessionId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Accept a pending booking as the instructor of that session.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.Confirm)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Confirm([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ConfirmSessionCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Refuse a pending booking as the instructor of that session.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.Decline)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Decline([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeclineSessionCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Cancel the session as its instructor, or give up the seat as a learner. Each side has its own
        /// lock before the start.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.Cancel)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelSessionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Confirm attendance by the learner for a completed session, releasing earnings instantly to the teacher's wallet.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.ConfirmAttendance)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmAttendance(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new LMS.Application.Features.Sessions.Commands.ConfirmAttendance.ConfirmAttendanceCommand { SessionId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Propose a new window for a session. Nothing moves until the other side answers.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Sessions.Reschedule)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reschedule(
            [FromRoute] Guid id,
            [FromBody] RequestRescheduleCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get the participants list and attendance records for a session taught by the calling instructor.
        /// </summary>
        [HttpGet(ApiRoutes.Sessions.Participants)]
        [RoleAuthorize(UserType.Instructor)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SessionParticipantDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetParticipants(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSessionParticipantsQuery { SessionId = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
