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

namespace LMS.Application.Features.InstructorProfile.Queries.GetInstructorProfile
{
    public class GetInstructorProfileQueryHandler
        : IRequestHandler<GetInstructorProfileQuery, Result<InstructorProfileDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetInstructorProfileQueryHandler(
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
            GetInstructorProfileQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.User)
                .Include(item => item.Application)
                .Include(item => item.Certificates)
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Authentication.UserNotFound);

            var profile = InstructorProfileMapper.ToDto(instructor);
            var roles = await _userManager.GetRolesAsync(instructor.User);

            profile.Profile = await UserProfileMapper.ToDtoAsync(
                _context, instructor.User, roles, cancellationToken);

            return Result<InstructorProfileDto>.Success(
                profile,
                _localizer[LocalizationKeys.InstructorProfile.ProfileRetrieved].Value);
        }
    }
}
