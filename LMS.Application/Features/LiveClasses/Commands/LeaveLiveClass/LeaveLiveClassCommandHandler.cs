using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.LeaveLiveClass
{
    public class LeaveLiveClassCommandHandler : IRequestHandler<LeaveLiveClassCommand, Result<bool>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public LeaveLiveClassCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(
            LeaveLiveClassCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.LiveRoom)
                    .ThenInclude(r => r!.Participants)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            if (session.LiveRoom == null)
            {
                return Result<bool>.Success(true, _localizer[LocalizationKeys.LiveClasses.RoomNotFound].Value);
            }

            var currentUserId = _currentUserService.UserId;
            var now = DateTime.UtcNow;

            var activeParticipants = session.LiveRoom.Participants
                .Where(p => p.UserId == currentUserId && p.LeftAtUtc == null)
                .ToList();

            foreach (var participant in activeParticipants)
            {
                participant.Leave(now);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true, _localizer[LocalizationKeys.LiveClasses.LeftSuccess].Value);
        }
    }
}
