using System;
using System.Collections.Generic;
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

namespace LMS.Application.Features.LiveClasses.Commands.StartLiveClass
{
    public class StartLiveClassCommandHandler : IRequestHandler<StartLiveClassCommand, Result<LiveClassTokenDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAgoraTokenService _agoraTokenService;
        private readonly IStringLocalizer<Messages> _localizer;

        public StartLiveClassCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IAgoraTokenService agoraTokenService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _agoraTokenService = agoraTokenService;
            _localizer = localizer;
        }

        public async Task<Result<LiveClassTokenDto>> Handle(
            StartLiveClassCommand request,
            CancellationToken cancellationToken)
        {
            if (!_agoraTokenService.IsConfigured)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Agora", _localizer[LocalizationKeys.LiveClasses.ServiceNotConfigured].Value)
                });
            }

            var session = await _context.Sessions
                .Include(s => s.Instructor)
                .Include(s => s.LiveRoom)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var currentUserId = _currentUserService.UserId;
            if (session.Instructor.UserId != currentUserId)
            {
                throw new ForbiddenException(LocalizationKeys.LiveClasses.OnlyInstructorCanStart);
            }

            if (session.Status != SessionStatus.Confirmed)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Session", _localizer[LocalizationKeys.LiveClasses.SessionMustBeConfirmed].Value)
                });
            }

            var now = DateTime.UtcNow;
            var joinOpensAt = session.StartsAtUtc.AddMinutes(-_agoraTokenService.JoinLeadMinutes);
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
                room = LiveRoom.Create(session.Id, currentUserId);
                await _context.LiveRooms.AddAsync(room, cancellationToken);
            }

            if (room.Status == LiveRoomStatus.Ended)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Room", _localizer[LocalizationKeys.LiveClasses.ClassAlreadyEnded].Value)
                });
            }

            if (room.Status == LiveRoomStatus.NotStarted)
            {
                room.Start(now);
            }

            uint agoraUid = _agoraTokenService.GenerateAgoraUid(currentUserId);

            var participant = LiveRoomParticipant.Create(
                room.Id,
                currentUserId,
                ParticipantRole.Host,
                agoraUid,
                now);

            await _context.LiveRoomParticipants.AddAsync(participant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            string token = _agoraTokenService.GenerateRtcToken(room.ChannelName, agoraUid, isPublisher: true);
            var expiresAtUtc = now.AddMinutes(1440);

            var dto = new LiveClassTokenDto
            {
                RoomId = room.Id,
                ChannelName = room.ChannelName,
                Uid = agoraUid,
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                Role = ParticipantRole.Host.ToString(),
                IsHost = true
            };

            return Result<LiveClassTokenDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.ClassStartedSuccess].Value);
        }
    }
}
