using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Queries.GetInstructorApplications
{
    public class GetInstructorApplicationsQueryHandler
        : IRequestHandler<GetInstructorApplicationsQuery, Result<PagginatedResult<InstructorApplicationDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorApplicationsQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<InstructorApplicationDto>>> Handle(
            GetInstructorApplicationsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.InstructorApplications
                .AsNoTracking()
                .Include(application => application.Instructor)
                    .ThenInclude(instructor => instructor.User)
                        .ThenInclude(user => user.Country)
                .Include(application => application.Instructor)
                    .ThenInclude(instructor => instructor.Certificates)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(application => application.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<InstructorApplicationDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<InstructorApplicationDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<InstructorApplicationDto>.MaxPageSize);

            var applications = await query
                .OrderByDescending(application => application.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = applications
                .Select(application => MapToDto(application))
                .ToArray();

            var result = PagginatedResult<InstructorApplicationDto>.Create(
                items,
                totalCount,
                pageNumber,
                pageSize);

            return Result<PagginatedResult<InstructorApplicationDto>>.Success(
                result,
                _localizer[LocalizationKeys.Authentication.InstructorApplicationsRetrieved].Value);
        }

        private InstructorApplicationDto MapToDto(InstructorApplication application)
        {
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

            return new InstructorApplicationDto(
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
        }
    }
}
