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

namespace LMS.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, Result<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<UserDto>> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),
                CountryId = request.CountryId,
                IsActive = request.IsActive
            };
            user.SetFullName(request.FullName.Trim());
            user.SetLanguage(Language.En);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BuildIdentityFailure(createResult);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToString());
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BuildIdentityFailure(roleResult);
            }

            var countryName = await _context.Countries
                .AsNoTracking()
                .Where(country => country.Id == user.CountryId)
                .Select(country => country.CountryEnName)
                .FirstOrDefaultAsync(cancellationToken);

            var response = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CountryId = user.CountryId,
                CountryName = countryName,
                Role = request.Role,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt
            };

            return Result<UserDto>.Success(
                response,
                _localizer[LocalizationKeys.Users.UserCreatedSuccess].Value,
                StatusCodes.Status201Created);
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
