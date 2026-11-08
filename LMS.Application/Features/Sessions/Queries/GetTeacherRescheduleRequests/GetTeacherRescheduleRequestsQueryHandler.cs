using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQueryHandler
        : IRequestHandler<GetTeacherRescheduleRequestsQuery, Result<PagginatedResult<TeacherRescheduleRequestDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetTeacherRescheduleRequestsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<TeacherRescheduleRequestDto>>> Handle(
            GetTeacherRescheduleRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var query = _context.RescheduleRequests
                .AsNoTracking()
                .Include(r => r.Session)
                    .ThenInclude(s => s.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                        .ThenInclude(b => b.Learner)
                            .ThenInclude(l => l.User)
                .Where(r => r.Session.InstructorId == instructorId);

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new TeacherRescheduleRequestDto
                {
                    Id = r.Id,
                    SessionId = r.SessionId,
                    SessionTitle = r.Session.Title,
                    CurrentStartsAtUtc = r.Session.StartsAtUtc,
                    CurrentEndsAtUtc = r.Session.EndsAtUtc,
                    ProposedStartsAtUtc = r.FromUtc,
                    ProposedEndsAtUtc = r.ToUtc,
                    Status = r.Status,
                    RequestedByType = r.RequestedByType,
                    IsRequestedByMe = r.RequestedByUserId == _currentUserService.UserId,
                    CreatedAt = r.CreatedAt,
                    Participants = r.Session.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                        .Select(b => new SessionParticipantSummaryDto
                        {
                            LearnerId = b.LearnerId,
                            FullName = b.Learner.User.FullName,
                            ProfilePictureUrl = b.Learner.User.ProfilePictureUrl
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            var paged = new PagginatedResult<TeacherRescheduleRequestDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagginatedResult<TeacherRescheduleRequestDto>>.Success(
                paged,
                _localizer[LocalizationKeys.Sessions.RescheduleRequestsRetrieved].Value);
        }
    }
}
