using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Commands.EndLiveClass
{
    public class EndLiveClassCommandHandler : IRequestHandler<EndLiveClassCommand, Result<bool>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public EndLiveClassCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(
            EndLiveClassCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Instructor)
                .Include(s => s.LiveRoom)
                    .ThenInclude(r => r!.Participants)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var currentUserId = _currentUserService.UserId;
            if (session.Instructor == null || session.Instructor.UserId != currentUserId)
            {
                throw new ForbiddenException(LocalizationKeys.LiveClasses.OnlyInstructorCanEnd);
            }

            if (session.LiveRoom == null)
            {
                return Result<bool>.Success(true, _localizer[LocalizationKeys.LiveClasses.RoomNotFound].Value);
            }

            var now = DateTime.UtcNow;
            session.LiveRoom.End(now);

            var openParticipants = session.LiveRoom.Participants
                .Where(p => p.LeftAtUtc == null)
                .ToList();

            foreach (var participant in openParticipants)
            {
                participant.Leave(now);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true, _localizer[LocalizationKeys.LiveClasses.ClassEndedSuccess].Value);
        }
    }
}
