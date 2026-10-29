using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Countries.Commands.UpdateCountry
{
    public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommand, Result<CountryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateCountryCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<CountryDto>> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Countries.CountryNotFound);

            country.CountryEnName = request.CountryEnName.Trim();
            country.Code = request.Code.Trim();
            country.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

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
                _localizer[LocalizationKeys.Countries.CountryUpdatedSuccess].Value);
        }
    }
}
