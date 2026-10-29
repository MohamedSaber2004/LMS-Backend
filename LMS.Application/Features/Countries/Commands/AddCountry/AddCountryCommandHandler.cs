using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Commands.AddCountry
{
    public class AddCountryCommandHandler : IRequestHandler<AddCountryCommand, Result<CountryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddCountryCommandHandler(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<CountryDto>> Handle(AddCountryCommand request, CancellationToken cancellationToken)
        {
            var country = new Country
            {
                CountryEnName = request.CountryEnName.Trim(),
                Code = request.Code.Trim()
            };

            await _context.Countries.AddAsync(country, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new CountryDto
            {
                Id = country.Id,
                CountryEnName = country.CountryEnName,
                Code = country.Code,
                IsActive = country.IsActive
            };

            return Result<CountryDto>.Success(
                response,
                _localizer[LocalizationKeys.Countries.CountryCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
