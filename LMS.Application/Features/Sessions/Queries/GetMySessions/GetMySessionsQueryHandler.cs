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

namespace LMS.Application.Features.Sessions.Queries.GetMySessions
{
    public class GetMySessionsQueryHandler
        : IRequestHandler<GetMySessionsQuery, Result<PagginatedResult<SessionDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMySessionsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<SessionDto>>> Handle(
            GetMySessionsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var query = SessionTabFilter.Apply(
                _context.Sessions
                    .AsNoTracking()
                    .Include(session => session.Instructor)
                        .ThenInclude(instructor => instructor.User)
                    .Where(session => session.InstructorId == instructorId),
                request.Tab);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<SessionDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<SessionDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<SessionDto>.MaxPageSize);

            var sessions = await query
                .OrderBy(session => session.StartsAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = sessions
                .Select(session => SessionMapper.ToDto(
                    session,
                    session.Instructor?.User?.FullName ?? string.Empty))
                .ToArray();

            return Result<PagginatedResult<SessionDto>>.Success(
                PagginatedResult<SessionDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.Sessions.SessionsRetrieved].Value);
        }
    }
}
