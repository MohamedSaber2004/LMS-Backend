using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
    {
        private const int CertificateFilePlace = (int)MediaKind.Document;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _fileValidator = fileValidator;
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
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),
                CountryId = request.CountryId,
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
                application = InstructorApplication.Create(
                    user.Id,
                    request.Specialization!.Trim(),
                    request.Bio!.Trim(),
                    request.YearsOfExperience!.Value,
                    NormalizeOptional(request.LinkedInUrl));

                foreach (var file in uploadedFiles)
                {
                    application.AddCertificate(InstructorCertificate.Create(
                        application.Id,
                        file.Path,
                        file.OriginalFileName,
                        file.Type));
                }

                await _context.InstructorApplications.AddAsync(application, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
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

        private async Task<List<UploadedFile>> UploadCertificatesAsync(List<IFormFile>? files)
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
                    InstructorCertificateType.Certificate));
            }

            return uploadedFiles;
        }

        private async Task CleanupFilesAsync(IEnumerable<UploadedFile> files)
        {
            foreach (var file in files)
            {
                await _fileValidator.DeleteFile(file.Path, CertificateFilePlace);
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

        private sealed record UploadedFile(string Path, string OriginalFileName, InstructorCertificateType Type);
    }
}
