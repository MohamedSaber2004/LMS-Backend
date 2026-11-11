using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Domain.Enums;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdQueryHandler
        : IRequestHandler<GetSessionByIdQuery, Result<SessionDetailDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetSessionByIdQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDetailDto>> Handle(
            GetSessionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .AsNoTracking()
                .Include(item => item.Instructor)
                    .ThenInclude(instructor => instructor.User)
                .Include(item => item.Bookings)
                    .ThenInclude(booking => booking.Learner)
                        .ThenInclude(learner => learner.User)
                .Include(item => item.RescheduleRequests)
                .Include(item => item.LiveRoom)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var ownInstructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            var isTeacher = ownInstructorId == session.InstructorId;

            var seesAllBookings = isTeacher || ActorOwnership.HasAdminBoardAccess(_currentUserService, AdminPermission.SessionsRead);

            var ownLearnerId = await ActorOwnership.ResolveLearnerIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            var myBooking = seesAllBookings
                ? null
                : session.Bookings.FirstOrDefault(booking => booking.LearnerId == ownLearnerId);

            var response = new SessionDetailDto
            {
                Session = SessionMapper.ToDto(
                    session,
                    session.Instructor?.User?.FullName ?? string.Empty,
                    myBooking),
                Bookings = seesAllBookings
                    ? session.Bookings
                        .OrderBy(booking => booking.CreatedAt)
                        .Select(booking => SessionMapper.ToBookingDto(
                            booking,
                            booking.Learner?.User?.FullName ?? string.Empty))
                        .ToArray()
                    : Array.Empty<SessionBookingDto>(),
                RescheduleRequests = session.RescheduleRequests
                    .OrderByDescending(item => item.CreatedAt)
                    .Select(SessionMapper.ToRescheduleDto)
                    .ToArray()
            };

            return Result<SessionDetailDto>.Success(
                response,
                _localizer[LocalizationKeys.Sessions.SessionRetrieved].Value);
        }
    }
}
