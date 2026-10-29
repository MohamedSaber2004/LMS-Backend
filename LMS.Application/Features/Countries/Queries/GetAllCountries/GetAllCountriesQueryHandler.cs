using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Queries.GetAllCountries
{
    public class GetAllCountriesQueryHandler
        : IRequestHandler<GetAllCountriesQuery, Result<IReadOnlyCollection<CountryDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAllCountriesQueryHandler(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<CountryDto>>> Handle(
            GetAllCountriesQuery request,
            CancellationToken cancellationToken)
        {
            var countries = await _context.Countries
                .AsNoTracking()
                .OrderBy(country => country.CountryEnName)
                .Select(country => new CountryDto
                {
                    Id = country.Id,
                    CountryEnName = country.CountryEnName,
                    Code = country.Code,
                    IsActive = country.IsActive
                })
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyCollection<CountryDto>>.Success(
                countries,
                _localizer[LocalizationKeys.Countries.CountriesRetrieved].Value);
        }
    }
}
