using System.Collections.Generic;
using FluentValidation.Results;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
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
        private readonly ILiveClassTokenService _liveClassTokenService;

        public ConfirmSessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer,
            ILiveClassTokenService liveClassTokenService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _liveClassTokenService = liveClassTokenService;
        }

        public async Task<Result<SessionDto>> Handle(
            ConfirmSessionCommand request,
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

            var existingRoom = await _context.LiveRooms
                .FirstOrDefaultAsync(r => r.SessionId == session.Id, cancellationToken);
            if (existingRoom == null)
            {
                var room = LiveRoom.Create(session.Id, session.Instructor.UserId);
                await _context.LiveRooms.AddAsync(room, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<SessionDto>.Success(
                SessionMapper.ToDto(session, session.Instructor?.User?.FullName ?? string.Empty),
                _localizer[LocalizationKeys.Sessions.SessionConfirmedSuccess].Value);
        }
    }
}
