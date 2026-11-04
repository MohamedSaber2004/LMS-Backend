using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.ResubmitInstructorApplication
{
    public class ResubmitInstructorApplicationCommandHandler
        : IRequestHandler<ResubmitInstructorApplicationCommand, Result<string>>
    {
        private const int CertificateFilePlace = (int)MediaKind.Document;

        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public ResubmitInstructorApplicationCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _fileValidator = fileValidator;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            ResubmitInstructorApplicationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var instructor = await _context.Instructors
                .Include(item => item.Application)
                .Include(item => item.Certificates)
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Authentication.UserNotFound);

            var application = instructor.Application
                ?? throw new NotFoundException(LocalizationKeys.Authentication.InstructorApplicationNotFound);

            if (application.Status != InstructorApplicationStatus.Rejected)
            {
                throw new BadRequestException(
                    LocalizationKeys.Authentication.InstructorApplicationNotResubmittable);
            }

            var uploadedPaths = new List<string>();

            try
            {
                foreach (var file in request.Certificates ?? new List<IFormFile>())
                {
                    var (uploaded, path) = await _fileValidator.UploadFile(file, CertificateFilePlace);
                    if (!uploaded)
                    {
                        throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest);
                    }

                    uploadedPaths.Add(path);
                    instructor.AddCertificate(InstructorCertificate.Create(
                        instructor.Id,
                        path,
                        Path.GetFileName(file.FileName),
                        InstructorCertificateType.Certificate));
                }

                instructor.UpdateProfile(
                    request.Specialization.Trim(),
                    request.Bio.Trim(),
                    request.YearsOfExperience!.Value,
                    request.Education!.Trim(),
                    NormalizeOptional(request.LinkedInUrl),
                    request.HourlyRate!.Value);

                application.Resubmit(userId, request.HourlyRate!.Value);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                foreach (var path in uploadedPaths)
                {
                    await _fileValidator.DeleteFile(path, CertificateFilePlace);
                }

                throw;
            }

            var message = _localizer[
                LocalizationKeys.Authentication.InstructorApplicationResubmitted].Value;

            return Result<string>.Success(message, message);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
