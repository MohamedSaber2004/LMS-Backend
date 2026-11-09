using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.JoinLiveClass
{
    public class JoinLiveClassCommandHandler : IRequestHandler<JoinLiveClassCommand, Result<LiveClassTokenDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILiveClassTokenService _liveClassTokenService;
        private readonly IStringLocalizer<Messages> _localizer;

        public JoinLiveClassCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            ILiveClassTokenService liveClassTokenService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _liveClassTokenService = liveClassTokenService;
            _localizer = localizer;
        }

        public async Task<Result<LiveClassTokenDto>> Handle(
            JoinLiveClassCommand request,
            CancellationToken cancellationToken)
        {
            if (!_liveClassTokenService.IsConfigured)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("LiveStreaming", _localizer[LocalizationKeys.LiveClasses.ServiceNotConfigured].Value)
                });
            }

            var session = await _context.Sessions
                .Include(s => s.Instructor)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                .Include(s => s.Lesson)
                    .ThenInclude(l => l!.Course)
                        .ThenInclude(c => c.Enrollments)
                            .ThenInclude(e => e.Learner)
                .Include(s => s.LiveRoom)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var currentUserId = _currentUserService.UserId;
            bool isHost = session.Instructor.UserId == currentUserId;
            bool isLearner = session.Bookings.Any(b =>
                b.Learner.UserId == currentUserId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Declined)
                || (session.Lesson != null && session.Lesson.Course.Enrollments.Any(e => e.Learner.UserId == currentUserId));

            if (!isHost && !isLearner)
            {
                throw new ForbiddenException(LocalizationKeys.LiveClasses.NotAuthorizedToJoin);
            }

            if (session.Status != SessionStatus.Confirmed)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Session", _localizer[LocalizationKeys.LiveClasses.SessionMustBeConfirmed].Value)
                });
            }

            var now = DateTime.UtcNow;
            var joinOpensAt = session.StartsAtUtc.AddMinutes(-_liveClassTokenService.JoinLeadMinutes);
            var joinClosesAt = session.EndsAtUtc.AddMinutes(30);

            if (now < joinOpensAt)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Time", string.Format(_localizer[LocalizationKeys.LiveClasses.JoinWindowNotOpen].Value, joinOpensAt.ToString("yyyy-MM-dd HH:mm")))
                });
            }

            if (now > joinClosesAt)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Time", _localizer[LocalizationKeys.LiveClasses.SessionWindowPassed].Value)
                });
            }

            var room = session.LiveRoom;
            if (room == null)
            {
                room = LiveRoom.Create(session.Id, session.Instructor.UserId);
                await _context.LiveRooms.AddAsync(room, cancellationToken);
            }

            if (room.Status == LiveRoomStatus.Ended)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Room", _localizer[LocalizationKeys.LiveClasses.ClassAlreadyEnded].Value)
                });
            }

            if (room.Status == LiveRoomStatus.NotStarted && isHost)
            {
                room.Start(now);
            }

            var role = isHost ? ParticipantRole.Host : ParticipantRole.Attendee;
            uint participantUid = _liveClassTokenService.GenerateParticipantUid(currentUserId);

            var participant = LiveRoomParticipant.Create(
                room.Id,
                currentUserId,
                role,
                participantUid,
                now);

            await _context.LiveRoomParticipants.AddAsync(participant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            string token = _liveClassTokenService.GenerateAccessToken(
                room.ChannelName,
                participantUid,
                canPublish: true);

            var expiresAtUtc = now.AddMinutes(_liveClassTokenService.TokenLifetimeMinutes);

            var dto = new LiveClassTokenDto
            {
                RoomId = room.Id,
                Url = _liveClassTokenService.Url,
                RoomName = room.ChannelName,
                Identity = participantUid.ToString(),
                Uid = participantUid,
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                Role = role.ToString(),
                IsHost = isHost
            };

            return Result<LiveClassTokenDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.JoinedSuccess].Value);
        }
    }
}
