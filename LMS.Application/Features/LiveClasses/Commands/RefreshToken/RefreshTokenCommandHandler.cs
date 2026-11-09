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
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILiveClassTokenService _liveClassTokenService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RefreshTokenCommandHandler(
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

        public async Task<Result<RefreshTokenDto>> Handle(
            RefreshTokenCommand request,
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
                .Include(s => s.LiveRoom)
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
                throw new ForbiddenException(LocalizationKeys.LiveClasses.NotAuthorizedToRefreshToken);
            }

            if (session.LiveRoom == null)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Room", _localizer[LocalizationKeys.LiveClasses.RoomNotFound].Value)
                });
            }

            if (session.LiveRoom.Status == LiveRoomStatus.Ended)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("Room", string.Format(_localizer[LocalizationKeys.LiveClasses.RoomNotActive].Value, session.LiveRoom.Status))
                });
            }

            uint participantUid = _liveClassTokenService.GenerateParticipantUid(currentUserId);
            string token = _liveClassTokenService.GenerateAccessToken(
                session.LiveRoom.ChannelName,
                participantUid,
                canPublish: true);

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_liveClassTokenService.TokenLifetimeMinutes);

            var dto = new RefreshTokenDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc
            };

            return Result<RefreshTokenDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.TokenRefreshedSuccess].Value);
        }
    }
}
