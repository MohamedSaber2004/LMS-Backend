using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Queries.GetInstructorApplicationById
{
    public class GetInstructorApplicationByIdQueryHandler
        : IRequestHandler<GetInstructorApplicationByIdQuery, Result<InstructorApplicationDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorApplicationByIdQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<InstructorApplicationDto>> Handle(
            GetInstructorApplicationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var application = await _context.InstructorApplications
                .AsNoTracking()
                .Include(a => a.Instructor)
                    .ThenInclude(i => i.User)
                        .ThenInclude(u => u.Country)
                .Include(a => a.Instructor)
                    .ThenInclude(i => i.Certificates)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (application == null)
            {
                return Result<InstructorApplicationDto>.Failure(
                    _localizer[LocalizationKeys.Authentication.InstructorApplicationNotFound].Value,
                    StatusCodes.Status404NotFound);
            }

            var instructor = application.Instructor;
            var user = instructor.User;
            var countryName = user.Country?.CountryEnName;

            var certificates = instructor.Certificates
                .Select(certificate => new InstructorCertificateDto(
                    certificate.Id,
                    certificate.FilePath,
                    certificate.OriginalFileName,
                    certificate.Type))
                .ToArray();

            var dto = new InstructorApplicationDto(
                application.Id,
                instructor.UserId,
                user.FullName,
                user.Email ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                user.CountryId,
                countryName,
                instructor.Specialization,
                instructor.Bio,
                instructor.YearsOfExperience,
                instructor.Education,
                instructor.LinkedInUrl,
                application.HourlyRate,
                application.Status,
                application.CreatedAt,
                application.ReviewedByUserId,
                application.ReviewedAt,
                application.ReviewNotes,
                certificates);

            return Result<InstructorApplicationDto>.Success(dto);
        }
    }
}
