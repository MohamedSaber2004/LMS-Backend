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

namespace LMS.Application.Features.Sessions.Commands.ConfirmSession
{
    public class ConfirmSessionCommandHandler
        : IRequestHandler<ConfirmSessionCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ConfirmSessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            ConfirmSessionCommand request,
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

            session.Confirm();

            // The learner asked; the teacher accepting is what turns both rows into a booking.
            foreach (var booking in session.Bookings
                         .Where(booking => booking.Status == BookingStatus.Pending))
            {
                booking.Confirm();
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, session.Instructor?.User?.FullName ?? string.Empty),
                _localizer[LocalizationKeys.Sessions.SessionConfirmedSuccess].Value);
        }
    }
}
