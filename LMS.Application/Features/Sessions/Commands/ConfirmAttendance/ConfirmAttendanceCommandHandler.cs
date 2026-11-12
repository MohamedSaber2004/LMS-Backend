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

namespace LMS.Application.Features.Sessions.Commands.ConfirmAttendance
{
    public class ConfirmAttendanceCommandHandler
        : IRequestHandler<ConfirmAttendanceCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ConfirmAttendanceCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            ConfirmAttendanceCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var session = await _context.Sessions
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Bookings)
                .Include(s => s.LiveRoom)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var booking = session.Bookings.FirstOrDefault(b => b.LearnerId == learnerId && b.Status != BookingStatus.Cancelled);
            if (booking == null)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            if (session.IsAttendanceConfirmed)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.AttendanceAlreadyConfirmed);
            }

            if (session.Status == SessionStatus.Cancelled)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.CannotConfirmCancelled);
            }

            var now = DateTime.UtcNow;
            session.ConfirmAttendance(now);

            await _context.SaveChangesAsync(cancellationToken);

            var dto = SessionMapper.ToDto(session, session.Instructor.User?.FullName ?? string.Empty, booking);
            return Result<SessionDto>.Success(dto, _localizer[LocalizationKeys.Sessions.AttendanceConfirmedSuccess].Value);
        }
    }
}

