using LMS.Application.Common.Helpers;
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

namespace LMS.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public VerifyOtpCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || user.IsDeleted)
            {
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.Authentication.UserNotFound].Value,
                    StatusCodes.Status400BadRequest);
            }

            if (!AuthenticationValidationHelper.IsOtpValid(user.VerificationCode, user.VerificationCodeExpiry, request.Otp))
            {
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.Authentication.OtpInvalidOrExpired].Value,
                    StatusCodes.Status400BadRequest);
            }

            var wasEmailUnverified = !user.EmailConfirmed;
            user.ClearVerificationCode();
            user.EmailConfirmed = true;

            var hasPendingInstructorApplication = wasEmailUnverified
                && await _context.InstructorApplications.AnyAsync(
                    application => application.Instructor.UserId == user.Id
                        && application.Status == InstructorApplicationStatus.Pending,
                    cancellationToken);

            if (wasEmailUnverified && !hasPendingInstructorApplication)
            {
                user.IsActive = true;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToArray();
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.ExceptionMessages.InternalServerError].Value,
                    StatusCodes.Status500InternalServerError,
                    errors);
            }

            var message = hasPendingInstructorApplication
                ? _localizer[LocalizationKeys.Authentication.InstructorEmailVerifiedPending].Value
                : _localizer[LocalizationKeys.Authentication.OtpVerifiedSuccess].Value;

            return Result<string>.Success(message, message);
        }
    }
}
