using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityPagginated
{
    public class GetInstructorAvailabilityPagginatedQueryHandler
        : IRequestHandler<GetInstructorAvailabilityPagginatedQuery, Result<PagginatedResult<InstructorAvailabilityRowDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorAvailabilityPagginatedQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<InstructorAvailabilityRowDto>>> Handle(
            GetInstructorAvailabilityPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.InstructorAvailabilitySlots
                .AsNoTracking()
                .Include(slot => slot.Instructor)
                    .ThenInclude(instructor => instructor.User)
                .AsQueryable();

            if (request.InstructorId.HasValue)
            {
                var instructorId = request.InstructorId.Value;
                query = query.Where(slot => slot.InstructorId == instructorId);
            }

            if (request.Day.HasValue)
            {
                var day = request.Day.Value;
                query = query.Where(slot => slot.Day == day);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<InstructorAvailabilityRowDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<InstructorAvailabilityRowDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<InstructorAvailabilityRowDto>.MaxPageSize);

            var slots = await query
                .OrderBy(slot => slot.InstructorId)
                .ThenBy(slot => slot.Day)
                .ThenBy(slot => slot.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = slots
                .Select(slot => AvailabilityMapper.ToRowDto(
                    slot,
                    slot.Instructor?.User?.FullName ?? string.Empty))
                .ToArray();

            var result = PagginatedResult<InstructorAvailabilityRowDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<InstructorAvailabilityRowDto>>.Success(
                result,
                _localizer[LocalizationKeys.Availability.SlotsRetrieved].Value);
        }
    }
}
