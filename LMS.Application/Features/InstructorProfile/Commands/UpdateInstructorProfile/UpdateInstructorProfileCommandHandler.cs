using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Localization;
using MediatR;
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

        public UpdateInstructorProfileCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
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

            instructor.UpdateProfile(
                request.Specialization.Trim(),
                request.Bio.Trim(),
                request.YearsOfExperience!.Value,
                request.Education!.Trim(),
                NormalizeOptional(request.LinkedInUrl),
                request.HourlyRate!.Value);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<InstructorProfileDto>.Success(
                InstructorProfileMapper.ToDto(instructor),
                _localizer[LocalizationKeys.InstructorProfile.ProfileUpdated].Value);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
