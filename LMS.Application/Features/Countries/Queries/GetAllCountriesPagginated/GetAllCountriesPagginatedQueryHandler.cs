using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Queries.GetAllCountriesPagginated
{
    public class GetAllCountriesPagginatedQueryHandler
        : IRequestHandler<GetAllCountriesPagginatedQuery, Result<PagginatedResult<CountryDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAllCountriesPagginatedQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<CountryDto>>> Handle(
            GetAllCountriesPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Countries
                .AsNoTracking()
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<CountryDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<CountryDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<CountryDto>.MaxPageSize);

            var countries = await query
                .OrderBy(country => country.CountryEnName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(country => new CountryDto
                {
                    Id = country.Id,
                    CountryEnName = country.CountryEnName,
                    Code = country.Code,
                    IsActive = country.IsActive
                })
                .ToListAsync(cancellationToken);

            var result = PagginatedResult<CountryDto>.Create(
                countries,
                totalCount,
                pageNumber,
                pageSize);

            return Result<PagginatedResult<CountryDto>>.Success(
                result,
                _localizer[LocalizationKeys.Countries.CountriesRetrieved].Value);
        }
    }
}
