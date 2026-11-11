using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Queries.GetPackageCatalog
{
    public class GetPackageCatalogQuery : IRequest<Result<IReadOnlyCollection<PackageCatalogItemDto>>>
    {
        public Guid? InstructorId { get; set; }
    }

    public class GetPackageCatalogQueryHandler : IRequestHandler<GetPackageCatalogQuery, Result<IReadOnlyCollection<PackageCatalogItemDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetPackageCatalogQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<PackageCatalogItemDto>>> Handle(
            GetPackageCatalogQuery request,
            CancellationToken cancellationToken)
        {
            decimal hourlyRate = 0m;
            if (request.InstructorId.HasValue && request.InstructorId.Value != Guid.Empty)
            {
                var instructor = await _context.Instructors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == request.InstructorId.Value, cancellationToken);
                if (instructor != null)
                {
                    hourlyRate = instructor.HourlyRate;
                }
            }

            var tiers = await _context.PackageTiers
                .AsNoTracking()
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync(cancellationToken);

            if (tiers.Count == 0)
            {
                throw new NotFoundException(LocalizationKeys.Packages.TiersUnavailable);
            }

            var packages = tiers
                .Select(t => new PackageCatalogItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TotalHours = t.TotalHours,
                    TotalFullSessions = t.TotalFullSessions,
                    TotalHalfSessions = t.TotalHalfSessions,
                    PricePerHour = hourlyRate,
                    TotalPrice = Math.Round(t.TotalHours * hourlyRate, 2),
                    Description = t.Description
                })
                .ToList();

            return Result<IReadOnlyCollection<PackageCatalogItemDto>>.Success(
                packages,
                _localizer[LocalizationKeys.Packages.CatalogRetrieved].Value);
        }
    }
}

