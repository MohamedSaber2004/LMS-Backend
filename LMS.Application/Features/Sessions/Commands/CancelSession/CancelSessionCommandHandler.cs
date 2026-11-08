using System.Collections.Generic;
using FluentValidation.Results;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.CancelSession
{
    public class CancelSessionCommandHandler
        : IRequestHandler<CancelSessionCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CancelSessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            CancelSessionCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(item => item.Instructor)
                    .ThenInclude(instructor => instructor.User)
                .Include(item => item.Bookings)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var liveRoom = await _context.LiveRooms
                .FirstOrDefaultAsync(r => r.SessionId == session.Id, cancellationToken);
            if (liveRoom != null && liveRoom.Status == LiveRoomStatus.Live)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("LiveRoom", _localizer[LocalizationKeys.LiveClasses.CannotCancelWhileLive].Value)
                });
            }

            var actor = await SessionGuards.ResolveActorAsync(
                _context, _currentUserService, session, cancellationToken);

            var actorUserId = _currentUserService.UserId;

            if (actor == SessionActor.Teacher)
            {
                foreach (var booking in HeldBookings(session))
                {
                    booking.Cancel(actorUserId, request.Reason);
                    session.ReleaseSeat();
                }

                session.Cancel(actorUserId, request.Reason);
            }
            else
            {
                var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                        _context, actorUserId, cancellationToken)
                    ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

                var booking = session.Bookings.FirstOrDefault(item =>
                    item.LearnerId == learnerId && Held(item));

                if (booking is null)
                {
                    throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
                }

                booking.Cancel(actorUserId, request.Reason);
                session.ReleaseSeat();

                // A one-to-one has no seat left to keep it alive once its only learner walks away.
                if (!session.Bookings.Any(Held))
                {
                    session.Cancel(actorUserId, request.Reason);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var myBooking = actor == SessionActor.Learner
                ? session.Bookings.FirstOrDefault(item => item.CancelledByUserId == actorUserId)
                : null;

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, session.Instructor?.User?.FullName ?? string.Empty, myBooking),
                _localizer[LocalizationKeys.Sessions.SessionCancelledSuccess].Value);
        }

        private static IEnumerable<SessionBooking> HeldBookings(Session session)
            => session.Bookings.Where(Held).ToArray();

        private static bool Held(SessionBooking booking)
            => booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed;
    }
}
