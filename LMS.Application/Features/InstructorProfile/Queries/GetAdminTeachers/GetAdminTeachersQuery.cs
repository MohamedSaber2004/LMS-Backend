using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Queries.GetAdminTeachers
{
    public class GetAdminTeachersQuery : IRequest<Result<PagginatedResult<AdminTeacherListItemDto>>>
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsNativeSpeaker { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<AdminTeacherListItemDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<AdminTeacherListItemDto>.DefaultPageSize;
    }

    public class GetAdminTeachersQueryHandler : IRequestHandler<GetAdminTeachersQuery, Result<PagginatedResult<AdminTeacherListItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminTeachersQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<AdminTeacherListItemDto>>> Handle(
            GetAdminTeachersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Instructors
                .AsNoTracking()
                .Include(i => i.User)
                    .ThenInclude(u => u.Country)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(i =>
                    i.User.FullName.ToLower().Contains(term) ||
                    (i.User.Email != null && i.User.Email.ToLower().Contains(term)) ||
                    i.Specialization.ToLower().Contains(term));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(i => i.User.IsActive == request.IsActive.Value);
            }

            if (request.IsNativeSpeaker.HasValue)
            {
                query = query.Where(i => i.IsNativeSpeaker == request.IsNativeSpeaker.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

            var instructors = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = instructors.Select(i => new AdminTeacherListItemDto
            {
                InstructorId = i.Id,
                UserId = i.UserId,
                FullName = i.User.FullName,
                Email = i.User.Email ?? string.Empty,
                PhoneNumber = i.User.PhoneNumber,
                Country = i.User.Country?.CountryEnName,
                Specialization = i.Specialization,
                HourlyRate = i.HourlyRate,
                CommissionRate = i.CommissionRate,
                IsNativeSpeaker = i.IsNativeSpeaker,
                Languages = i.GetLanguagesList(),
                RatingAverage = i.RatingAverage,
                TotalReviews = i.TotalReviews,
                CompletedSessionsCount = i.CompletedSessionsCount,
                IsActive = i.User.IsActive,
                JoinedAtUtc = i.CreatedAt
            }).ToList();

            return Result<PagginatedResult<AdminTeacherListItemDto>>.Success(
                PagginatedResult<AdminTeacherListItemDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.AdminManagement.TeachersRetrievedSuccess].Value);
        }
    }
}
