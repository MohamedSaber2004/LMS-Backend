using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentLanguageService _currentLanguageService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            ICurrentLanguageService currentLanguageService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
            _currentLanguageService = currentLanguageService;
            _localizer = localizer;
        }

        public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString())
                ?? throw new UnAuthorizedException();

            var roles = await _userManager.GetRolesAsync(user);
            var country = user.CountryId.HasValue
                ? await _context.Countries.FirstOrDefaultAsync(c => c.Id == user.CountryId.Value, cancellationToken)
                : null;

            var countryName = country is null
                ? null
                : _currentLanguageService.Language == Language.Ar
                    ? country.CountryArName
                    : country.CountryEnName;

            var profile = new UserProfileDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.ProfilePictureUrl,
                user.CountryId,
                countryName,
                user.Language,
                roles,
                user.CreatedAt);

            return Result<UserProfileDto>.Success(
                profile,
                _localizer[LocalizationKeys.Authentication.ProfileRetrievedSuccess].Value);
        }
    }
}
