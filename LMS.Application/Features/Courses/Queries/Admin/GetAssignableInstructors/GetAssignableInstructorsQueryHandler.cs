using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Queries.Admin.GetAssignableInstructors
{
    public class GetAssignableInstructorsQueryHandler
        : IRequestHandler<GetAssignableInstructorsQuery, Result<PagginatedResult<InstructorRefDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAssignableInstructorsQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<InstructorRefDto>>> Handle(
            GetAssignableInstructorsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Instructors
                .AsNoTracking()
                .Where(instructor => instructor.IsActive
                    && instructor.Application != null
                    && instructor.Application.Status == InstructorApplicationStatus.Approved
                    && instructor.User.IsActive
                    && !instructor.User.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(instructor =>
                    instructor.User.FullName.Contains(search)
                    || instructor.Specialization.Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<InstructorRefDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<InstructorRefDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<InstructorRefDto>.MaxPageSize);

            var items = await query
                .OrderBy(instructor => instructor.User.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(instructor => new InstructorRefDto
                {
                    Id = instructor.Id,
                    UserId = instructor.UserId,
                    FullName = instructor.User.FullName,
                    Specialization = instructor.Specialization
                })
                .ToListAsync(cancellationToken);

            var result = PagginatedResult<InstructorRefDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<InstructorRefDto>>.Success(
                result,
                _localizer[LocalizationKeys.Courses.InstructorsRetrieved].Value);
        }
    }
}
