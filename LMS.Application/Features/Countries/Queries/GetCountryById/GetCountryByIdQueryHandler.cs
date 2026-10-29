using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetCountryByIdQueryHandler(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
        {
            var country = await _context.Countries
                .AsNoTracking()
                .Where(item => item.Id == request.Id)
                .Select(item => new CountryDto
                {
                    Id = item.Id,
                    CountryEnName = item.CountryEnName,
                    Code = item.Code,
                    IsActive = item.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Countries.CountryNotFound);

            return Result<CountryDto>.Success(
                country,
                _localizer[LocalizationKeys.Countries.CountryRetrieved].Value);
        }
    }
}
