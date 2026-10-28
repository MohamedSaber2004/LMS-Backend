using System.Security.Cryptography;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Authentication.Commands.ResendOtp
{
    public class ResendOtpCommandHandler : IRequestHandler<ResendOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IStringLocalizer<Messages> _localizer;

        public ResendOtpCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<EmailSettings> emailOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailSettings = emailOptions?.Value ?? new EmailSettings();
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expiryMinutes = _emailSettings.VerificationCodeExpiryMinutes > 0 ? _emailSettings.VerificationCodeExpiryMinutes : 10;
            var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            user!.SetVerificationCode(otp, expiry);
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToArray();
                return Result<string>.Failure(
                    _localizer[LocalizationKeys.ExceptionMessages.InternalServerError].Value,
                    StatusCodes.Status500InternalServerError,
                    errors);
            }

            var subject = _localizer[LocalizationKeys.Authentication.ResendOtpEmailSubject].Value;
            var message = _localizer[LocalizationKeys.Authentication.ResendOtpEmailBody].Value;
            var defaultUserName = _localizer[LocalizationKeys.Authentication.DefaultUserName].Value;
            var userName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : defaultUserName;

            await _emailService.SendOtpEmailAsync(
                user.Email!,
                userName,
                subject,
                message,
                otp,
                expiryMinutes,
                cancellationToken);

            var successMessage = _localizer[LocalizationKeys.Authentication.OtpResentSuccess].Value;
            return Result<string>.Success(successMessage, successMessage);
        }
    }
}
