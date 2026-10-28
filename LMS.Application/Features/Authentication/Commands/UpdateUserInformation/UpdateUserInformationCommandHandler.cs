using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.UpdateUserInformation
{
    public class UpdateUserInformationCommandHandler : IRequestHandler<UpdateUserInformationCommand, Result<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentLanguageService _currentLanguageService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateUserInformationCommandHandler(
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

        public async Task<Result<UserProfileDto>> Handle(UpdateUserInformationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString())
                ?? throw new UnAuthorizedException();

            user.SetFullName(request.FullName.Trim());

            if (request.CountryId.HasValue)
            {
                user.CountryId = request.CountryId.Value;
            }

            if (request.Language.HasValue)
            {
                user.SetLanguage(request.Language.Value);
            }

            if (request.ProfilePictureUrl is not null)
            {
                user.SetProfilePictureUrl(string.IsNullOrWhiteSpace(request.ProfilePictureUrl)
                    ? null
                    : request.ProfilePictureUrl.Trim());
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToArray();
                return Result<UserProfileDto>.Failure(
                    _localizer[LocalizationKeys.ExceptionMessages.BadRequest].Value,
                    StatusCodes.Status400BadRequest,
                    errors);
            }

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
                _localizer[LocalizationKeys.Authentication.ProfileUpdatedSuccess].Value);
        }
    }
}
