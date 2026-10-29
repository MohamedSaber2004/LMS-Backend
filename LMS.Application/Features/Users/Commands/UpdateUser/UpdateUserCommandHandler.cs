using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Extensions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString())
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            var email = request.Email?.Trim();
            var emailChanged = email is not null
                && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
            var wasActive = user.IsActive;
            var deactivated = wasActive && request.IsActive == false;

            if (request.FullName is not null)
            {
                user.SetFullName(request.FullName.Trim());
            }

            if (email is not null)
            {
                user.Email = email;
                user.UserName = email;
            }

            if (request.PhoneNumber is not null)
            {
                user.PhoneNumber = request.PhoneNumber.Trim();
            }

            if (request.CountryId.HasValue)
            {
                user.CountryId = request.CountryId.Value;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BuildIdentityFailure(updateResult);
            }

            var roleChanged = false;

            if (request.Role.HasValue)
            {
                var roleName = request.Role.Value.ToString();
                var currentRoles = await _userManager.GetRolesAsync(user);
                var hasTargetRole = currentRoles.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
                roleChanged = !hasTargetRole || currentRoles.Count > 1;

                if (!hasTargetRole)
                {
                    var addResult = await _userManager.AddToRoleAsync(user, roleName);
                    if (!addResult.Succeeded)
                    {
                        return BuildIdentityFailure(addResult);
                    }
                }

                if (roleChanged)
                {
                    var rolesToRemove = currentRoles
                        .Where(role => !string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (rolesToRemove.Length > 0)
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeResult.Succeeded)
                        {
                            return BuildIdentityFailure(removeResult);
                        }
                    }
                }
            }

            if (roleChanged || deactivated || emailChanged)
            {
                user.IncrementTokenVersion();

                var versionResult = await _userManager.UpdateAsync(user);
                if (!versionResult.Succeeded)
                {
                    return BuildIdentityFailure(versionResult);
                }

                await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var countryName = await _context.Countries
                .AsNoTracking()
                .Where(country => country.Id == user.CountryId)
                .Select(country => country.CountryEnName)
                .FirstOrDefaultAsync(cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);
            UserType? currentRole = null;

            if (Enum.TryParse<UserType>(roles.FirstOrDefault(), ignoreCase: true, out var parsedRole))
            {
                currentRole = parsedRole;
            }

            var response = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CountryId = user.CountryId,
                CountryName = countryName,
                Role = currentRole,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt
            };

            return Result<UserDto>.Success(
                response,
                _localizer[LocalizationKeys.Users.UserUpdatedSuccess].Value);
        }

        private Result<UserDto> BuildIdentityFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToArray();

            return Result<UserDto>.Failure(
                _localizer[LocalizationKeys.ExceptionMessages.BadRequest].Value,
                StatusCodes.Status400BadRequest,
                errors);
        }
    }
}
