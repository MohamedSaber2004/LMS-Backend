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

namespace LMS.Application.Features.Sessions.Commands.DeclineSession
{
    public class DeclineSessionCommandHandler
        : IRequestHandler<DeclineSessionCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeclineSessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            DeclineSessionCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(item => item.Instructor)
                    .ThenInclude(instructor => instructor.User)
                .Include(item => item.Bookings)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            await SessionGuards.EnsureTeacherAsync(
                _context, _currentUserService, session, cancellationToken);

            var actorUserId = _currentUserService.UserId;

            foreach (var booking in session.Bookings
                         .Where(booking => booking.Status == BookingStatus.Pending))
            {
                booking.Decline(actorUserId);
                session.ReleaseSeat();
            }

            session.Cancel(actorUserId, null);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, session.Instructor?.User?.FullName ?? string.Empty),
                _localizer[LocalizationKeys.Sessions.SessionDeclinedSuccess].Value);
        }
    }
}
