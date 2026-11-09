using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Application.Features.LiveClasses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.LiveClasses.Queries.GetLiveClassSession
{
    public class GetLiveClassSessionQueryHandler : IRequestHandler<GetLiveClassSessionQuery, Result<LiveClassDetailsDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILiveClassTokenService _liveClassTokenService;
        private readonly SchedulingOptions _scheduling;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetLiveClassSessionQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            ILiveClassTokenService liveClassTokenService,
            IOptions<SchedulingOptions> schedulingOptions,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _liveClassTokenService = liveClassTokenService;
            _scheduling = schedulingOptions.Value;
            _localizer = localizer;
        }

        public async Task<Result<LiveClassDetailsDto>> Handle(
            GetLiveClassSessionQuery request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Include(s => s.LiveRoom!)
                    .ThenInclude(r => r.Participants)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var currentUserId = _currentUserService.UserId;
            bool isHost = session.Instructor.UserId == currentUserId;
            bool isLearner = session.Bookings.Any(b =>
                b.Learner.UserId == currentUserId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Declined);

            if (!isHost && !isLearner)
            {
                throw new ForbiddenException(LocalizationKeys.LiveClasses.NotAuthorizedToView);
            }

            var now = DateTime.UtcNow;
            int leadMinutes = _liveClassTokenService.JoinLeadMinutes;
            var joinOpensAtUtc = session.StartsAtUtc.AddMinutes(-leadMinutes);
            var joinClosesAtUtc = session.EndsAtUtc.AddMinutes(_scheduling.JoinTailMinutes);

            var room = session.LiveRoom;
            var roomStatus = room != null ? room.Status : LiveRoomStatus.NotStarted;

            bool canJoinNow = session.Status == SessionStatus.Confirmed &&
                              roomStatus != LiveRoomStatus.Ended &&
                              now >= joinOpensAtUtc &&
                              now <= joinClosesAtUtc;

            var participants = room?.Participants
                .OrderByDescending(p => p.JoinedAtUtc)
                .Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    Role = p.Role.ToString(),
                    Uid = p.AgoraUid,
                    JoinedAtUtc = p.JoinedAtUtc,
                    IsOnline = p.LeftAtUtc == null
                })
                .ToList() ?? new List<ParticipantDto>();

            var dto = new LiveClassDetailsDto
            {
                SessionId = session.Id,
                RoomId = room?.Id,
                Url = _liveClassTokenService.Url,
                RoomName = room?.ChannelName ?? $"live-{session.Id:N}",
                Status = roomStatus.ToString(),
                CanJoinNow = canJoinNow,
                JoinOpensAtUtc = joinOpensAtUtc,
                StartsAtUtc = session.StartsAtUtc,
                EndsAtUtc = session.EndsAtUtc,
                StartedAtUtc = room?.StartedAtUtc,
                EndedAtUtc = room?.EndedAtUtc,
                IsHost = isHost,
                ActiveParticipantsCount = participants.Count(p => p.IsOnline),
                Participants = participants
            };

            return Result<LiveClassDetailsDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.SessionDetailsRetrieved].Value);
        }
    }
}
