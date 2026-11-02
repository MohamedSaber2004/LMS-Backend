using System.Security.Cryptography;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LMS.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
    {
        private const int CertificateFilePlace = (int)MediaKind.Document;
        private const int IntroVideoFilePlace = (int)MediaKind.Video;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IFileValidator _fileValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IStringLocalizer<Messages> _localizer;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IFileValidator fileValidator,
            IVideoValidator videoValidator,
            IEmailService emailService,
            IOptions<EmailSettings> emailOptions,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _fileValidator = fileValidator;
            _videoValidator = videoValidator;
            _emailService = emailService;
            _emailSettings = emailOptions?.Value ?? new EmailSettings();
            _localizer = localizer;
        }

        public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var isInstructor = request.Role == UserType.Instructor;
            var email = request.Email.Trim();
            var uploadedFiles = new List<UploadedFile>();

            if (isInstructor)
            {
                uploadedFiles.AddRange(await UploadCertificatesAsync(request.Certificates));
                uploadedFiles.AddRange(await UploadIntroVideoAsync(request.IntroVideo));
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = PhoneNumberValidationHelper.Normalize(request.PhoneNumber),
                CountryId = request.CountryId,
                EmailConfirmed = false,
                IsActive = !isInstructor
            };
            user.SetFullName(request.FullName.Trim());
            user.SetLanguage(Language.En);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await CleanupFilesAsync(uploadedFiles);
                return BuildIdentityFailure(createResult);
            }

            if (!isInstructor)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, nameof(UserType.Learner));
                if (!roleResult.Succeeded)
                {
                    await DeleteUserAsync(user);
                    return BuildIdentityFailure(roleResult);
                }

                try
                {
                    await _context.Learners.AddAsync(Learner.Create(user.Id), cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    await DeleteUserAsync(user);
                    throw;
                }

                var learnerResponse = new RegisterResponseDto(
                    user.Id,
                    user.Email!,
                    user.FullName,
                    UserType.Learner,
                    requiresApproval: false);

                return Result<RegisterResponseDto>.Success(
                    learnerResponse,
                    _localizer[LocalizationKeys.Authentication.RegistrationSuccess].Value,
                    StatusCodes.Status201Created);
            }

            InstructorApplication application;
            try
            {
                var instructor = Instructor.Create(
                    user.Id,
                    request.Specialization!.Trim(),
                    request.Bio!.Trim(),
                    request.YearsOfExperience!.Value,
                    request.Education!.Trim(),
                    NormalizeOptional(request.LinkedInUrl));

                foreach (var file in uploadedFiles)
                {
                    instructor.AddCertificate(InstructorCertificate.Create(
                        instructor.Id,
                        file.Path,
                        file.OriginalFileName,
                        file.Type));
                }

                application = InstructorApplication.Create(instructor.Id);

                await _context.Instructors.AddAsync(instructor, cancellationToken);
                await _context.InstructorApplications.AddAsync(application, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await DeleteUserAsync(user);
                await CleanupFilesAsync(uploadedFiles);
                throw;
            }

            try
            {
                await SendVerificationEmailAsync(user, cancellationToken);
            }
            catch
            {
                await DeleteUserAsync(user);
                await CleanupFilesAsync(uploadedFiles);
                throw;
            }

            var instructorResponse = new RegisterResponseDto(
                user.Id,
                user.Email!,
                user.FullName,
                UserType.Instructor,
                requiresApproval: true,
                application.Id);

            return Result<RegisterResponseDto>.Success(
                instructorResponse,
                _localizer[LocalizationKeys.Authentication.InstructorRegistrationPending].Value,
                StatusCodes.Status202Accepted);
        }

        private async Task SendVerificationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expiryMinutes = _emailSettings.VerificationCodeExpiryMinutes > 0
                ? _emailSettings.VerificationCodeExpiryMinutes
                : 10;

            user.SetVerificationCode(code, DateTime.UtcNow.AddMinutes(expiryMinutes));
            await _userManager.UpdateAsync(user);

            var subject = _localizer[LocalizationKeys.Authentication.RegistrationCodeEmailSubject].Value;
            var message = _localizer[LocalizationKeys.Authentication.RegistrationCodeEmailBody].Value;
            var defaultUserName = _localizer[LocalizationKeys.Authentication.DefaultUserName].Value;
            var userName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : defaultUserName;

            await _emailService.SendOtpEmailAsync(
                user.Email!,
                userName,
                subject,
                message,
                code,
                expiryMinutes,
                cancellationToken);
        }

        private async Task<List<UploadedFile>> UploadCertificatesAsync(List<IFormFile> files)
        {
            var uploadedFiles = new List<UploadedFile>();

            if (files is null)
            {
                return uploadedFiles;
            }

            foreach (var file in files)
            {
                var (uploaded, result) = await _fileValidator.UploadFile(file, CertificateFilePlace);
                if (!uploaded)
                {
                    await CleanupFilesAsync(uploadedFiles);
                    throw new InvalidOperationException(result);
                }

                uploadedFiles.Add(new UploadedFile(
                    result,
                    Path.GetFileName(file.FileName),
                    InstructorCertificateType.Certificate,
                    CertificateFilePlace));
            }

            return uploadedFiles;
        }

        private async Task<List<UploadedFile>> UploadIntroVideoAsync(IFormFile? introVideo)
        {
            var uploadedFiles = new List<UploadedFile>();

            if (introVideo is null || introVideo.Length == 0)
            {
                return uploadedFiles;
            }

            var (uploaded, result) = await _videoValidator.UploadVideo(introVideo, IntroVideoFilePlace);
            if (!uploaded)
            {
                await CleanupFilesAsync(uploadedFiles);
                throw new InvalidOperationException(result);
            }

            uploadedFiles.Add(new UploadedFile(
                result,
                Path.GetFileName(introVideo.FileName),
                InstructorCertificateType.IntroVideo,
                IntroVideoFilePlace));

            return uploadedFiles;
        }

        private async Task CleanupFilesAsync(IEnumerable<UploadedFile> files)
        {
            foreach (var file in files)
            {
                if (file.Place == IntroVideoFilePlace)
                {
                    await _videoValidator.DeleteVideo(file.Path, file.Place);
                }
                else
                {
                    await _fileValidator.DeleteFile(file.Path, file.Place);
                }
            }
        }

        private async Task DeleteUserAsync(ApplicationUser user)
        {
            try
            {
                await _userManager.DeleteAsync(user);
            }
            catch
            {
            }
        }

        private Result<RegisterResponseDto> BuildIdentityFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToArray();

            return Result<RegisterResponseDto>.Failure(
                _localizer[LocalizationKeys.ExceptionMessages.BadRequest].Value,
                StatusCodes.Status400BadRequest,
                errors);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed record UploadedFile(string Path, string OriginalFileName, InstructorCertificateType Type, int Place);
    }
}
