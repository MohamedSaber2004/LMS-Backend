using LMS.API.Services;
using LMS.Application.Common.Helpers;
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
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILmsDbContext _context;
        private readonly IUserAccountDeletionService _accountDeletionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ILmsDbContext context,
            IUserAccountDeletionService accountDeletionService,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _accountDeletionService = accountDeletionService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<UserDto>> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);
            existingUser ??= await _userManager.FindByNameAsync(email);
            if (existingUser is not null && existingUser.IsDeleted)
            {
                await _accountDeletionService.HardDeleteUserAsync(existingUser.Id, cancellationToken);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = PhoneNumberValidationHelper.Normalize(request.PhoneNumber),
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

            await IdentityRoleProvisioner.EnsureRoleAsync(_roleManager, request.Role.ToString());

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToString());
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BuildIdentityFailure(roleResult);
            }

            if (request.Role == UserType.Learner)
            {
                await _context.Learners.AddAsync(Learner.Create(user.Id), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else if (request.Role == UserType.Instructor)
            {
                var instructor = Instructor.Create(user.Id, string.Empty, string.Empty, 0, string.Empty, null, 0m);
                var application = InstructorApplication.Create(instructor.Id, 0m);
                application.Approve(_currentUserService.UserId, null);

                await _context.Instructors.AddAsync(instructor, cancellationToken);
                await _context.InstructorApplications.AddAsync(application, cancellationToken);
                await _context.TeacherWallets.AddAsync(TeacherWallet.Create(instructor.Id), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
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
