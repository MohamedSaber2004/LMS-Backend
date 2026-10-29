using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication
{
    public class ApproveInstructorApplicationCommandHandler
        : IRequestHandler<ApproveInstructorApplicationCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ApproveInstructorApplicationCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            ApproveInstructorApplicationCommand request,
            CancellationToken cancellationToken)
        {
            var adminUserId = _currentUserService.UserId;
            if (adminUserId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var application = await _context.InstructorApplications
                .Include(item => item.User)
                .FirstOrDefaultAsync(item => item.Id == request.ApplicationId, cancellationToken)
                ?? throw new NotFoundException(
                    LocalizationKeys.Authentication.InstructorApplicationNotFound);

            if (application.Status != InstructorApplicationStatus.Pending)
            {
                throw new BadRequestException(
                    LocalizationKeys.Authentication.InstructorApplicationAlreadyReviewed);
            }

            var user = application.User
                ?? await _userManager.FindByIdAsync(application.UserId.ToString());

            if (user is null || user.IsDeleted)
            {
                throw new NotFoundException(LocalizationKeys.Authentication.UserNotFound);
            }

            user.IsActive = true;
            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BuildIdentityFailure(updateResult);
            }

            if (!await _userManager.IsInRoleAsync(user, nameof(UserType.Instructor)))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, nameof(UserType.Instructor));
                if (!roleResult.Succeeded)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                    return BuildIdentityFailure(roleResult);
                }
            }

            application.Approve(adminUserId, NormalizeOptional(request.ReviewNotes));
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.InstructorApprovedSuccess].Value;
            return Result<string>.Success(message, message);
        }

        private Result<string> BuildIdentityFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToArray();

            return Result<string>.Failure(
                _localizer[LocalizationKeys.ExceptionMessages.BadRequest].Value,
                StatusCodes.Status400BadRequest,
                errors);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
