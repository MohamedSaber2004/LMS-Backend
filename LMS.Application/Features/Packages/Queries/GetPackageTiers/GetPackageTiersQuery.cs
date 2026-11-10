using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Queries.GetPackageTiers
{
    public class GetPackageTiersQuery : IRequest<Result<IReadOnlyCollection<PackageTierDto>>>
    {
    }

    public class GetPackageTiersQueryHandler : IRequestHandler<GetPackageTiersQuery, Result<IReadOnlyCollection<PackageTierDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetPackageTiersQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<PackageTierDto>>> Handle(
            GetPackageTiersQuery request,
            CancellationToken cancellationToken)
        {
            var tiers = await _context.PackageTiers
                .AsNoTracking()
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync(cancellationToken);

            var items = tiers.Select(PackageTierMapper.ToDto).ToList();

            return Result<IReadOnlyCollection<PackageTierDto>>.Success(
                items,
                _localizer[LocalizationKeys.Packages.TiersRetrieved].Value);
        }
    }
}
