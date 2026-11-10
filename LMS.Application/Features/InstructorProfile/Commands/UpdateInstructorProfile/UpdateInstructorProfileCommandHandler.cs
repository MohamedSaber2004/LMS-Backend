using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Commands.UpdateInstructorProfile
{
    public class UpdateInstructorProfileCommandHandler
        : IRequestHandler<UpdateInstructorProfileCommand, Result<InstructorProfileDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateInstructorProfileCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _userManager = userManager;
        }

        public async Task<Result<InstructorProfileDto>> Handle(
            UpdateInstructorProfileCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var instructor = await _context.Instructors
                .Include(item => item.User)
                .Include(item => item.Application)
                .Include(item => item.Certificates)
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Authentication.UserNotFound);

            var user = instructor.User;

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.SetFullName(request.FullName.Trim());
            }

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
                user.SetProfilePictureUrl(NormalizeOptional(request.ProfilePictureUrl));
            }

            instructor.UpdateProfile(
                request.Specialization.Trim(),
                request.Bio.Trim(),
                request.YearsOfExperience!.Value,
                request.Education!.Trim(),
                NormalizeOptional(request.LinkedInUrl));

            await _context.SaveChangesAsync(cancellationToken);

            var profile = InstructorProfileMapper.ToDto(instructor);
            var roles = await _userManager.GetRolesAsync(user);

            profile.Profile = await UserProfileMapper.ToDtoAsync(
                _context, user, roles, cancellationToken);

            return Result<InstructorProfileDto>.Success(
                profile,
                _localizer[LocalizationKeys.InstructorProfile.ProfileUpdated].Value);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
