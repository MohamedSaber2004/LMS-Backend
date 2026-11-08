using System.Collections.Generic;
using FluentValidation.Results;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.ApproveReschedule
{
    public class ApproveRescheduleCommandHandler
        : IRequestHandler<ApproveRescheduleCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ApproveRescheduleCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            ApproveRescheduleCommand request,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .Include(item => item.Session)
                    .ThenInclude(session => session.Instructor)
                        .ThenInclude(instructor => instructor.User)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.RescheduleNotFound);

            var session = reschedule.Session;

            var liveRoom = await _context.LiveRooms
                .FirstOrDefaultAsync(r => r.SessionId == session.Id, cancellationToken);
            if (liveRoom != null && (liveRoom.Status == LiveRoomStatus.Live || liveRoom.Status == LiveRoomStatus.Ended))
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("LiveRoom", _localizer[LocalizationKeys.LiveClasses.CannotRescheduleStartedOrEnded].Value)
                });
            }

            session.MoveTo(reschedule.FromUtc, reschedule.ToUtc);
            reschedule.Approve(_currentUserService.UserId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, session.Instructor?.User?.FullName ?? string.Empty),
                _localizer[LocalizationKeys.Sessions.RescheduleApprovedSuccess].Value);
        }
    }
}
