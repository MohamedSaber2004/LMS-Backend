using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQueryHandler
        : IRequestHandler<GetSessionParticipantsQuery, Result<IReadOnlyCollection<SessionParticipantDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetSessionParticipantsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<SessionParticipantDto>>> Handle(
            GetSessionParticipantsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var session = await _context.Sessions
                .AsNoTracking()
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Include(s => s.Attendances)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            if (session.InstructorId != instructorId)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            var participants = session.Bookings.Select(booking =>
            {
                var attendance = session.Attendances.FirstOrDefault(a => a.LearnerId == booking.LearnerId);
                return new SessionParticipantDto
                {
                    BookingId = booking.Id,
                    LearnerId = booking.LearnerId,
                    UserId = booking.Learner.UserId,
                    FullName = booking.Learner.User.FullName,
                    ProfilePictureUrl = booking.Learner.User.ProfilePictureUrl,
                    BookingStatus = booking.Status,
                    BookedAt = booking.CreatedAt,
                    AttendanceStatus = attendance?.Status,
                    JoinedAtUtc = attendance?.JoinedAtUtc,
                    LeftAtUtc = attendance?.LeftAtUtc,
                    DurationMinutes = attendance?.DurationMinutes,
                    AttendanceNotes = attendance?.Notes
                };
            }).ToList();

            return Result<IReadOnlyCollection<SessionParticipantDto>>.Success(
                participants,
                _localizer[LocalizationKeys.Sessions.ParticipantsRetrieved].Value);
        }
    }
}
