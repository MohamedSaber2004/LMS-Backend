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

namespace LMS.Application.Features.Sessions.Queries.GetMyBookings
{
    public class GetMyBookingsQueryHandler
        : IRequestHandler<GetMyBookingsQuery, Result<PagginatedResult<SessionDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyBookingsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<SessionDto>>> Handle(
            GetMyBookingsQuery request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var query = SessionTabFilter.ApplyToBookings(
                _context.SessionBookings
                    .AsNoTracking()
                    .Include(booking => booking.Session)
                        .ThenInclude(session => session.Instructor)
                            .ThenInclude(instructor => instructor.User)
                    .Include(booking => booking.Session)
                        .ThenInclude(session => session.LiveRoom)
                    .Where(booking => booking.LearnerId == learnerId),
                request.Tab);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<SessionDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<SessionDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<SessionDto>.MaxPageSize);

            var bookings = await query
                .OrderBy(booking => booking.Session.StartsAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = bookings
                .Select(booking => SessionMapper.ToDto(
                    booking.Session,
                    booking.Session.Instructor?.User?.FullName ?? string.Empty,
                    booking))
                .ToArray();

            return Result<PagginatedResult<SessionDto>>.Success(
                PagginatedResult<SessionDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.Sessions.BookingsRetrieved].Value);
        }
    }
}
