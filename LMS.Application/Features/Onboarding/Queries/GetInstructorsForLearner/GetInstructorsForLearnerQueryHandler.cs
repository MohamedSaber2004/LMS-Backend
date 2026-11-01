using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Queries.GetInstructorsForLearner
{
    public class GetInstructorsForLearnerQueryHandler
        : IRequestHandler<GetInstructorsForLearnerQuery, Result<PagginatedResult<InstructorCardDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorsForLearnerQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<InstructorCardDto>>> Handle(
            GetInstructorsForLearnerQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.InstructorApplications
                .AsNoTracking()
                .Include(application => application.User)
                    .ThenInclude(user => user.Country)
                .Include(application => application.Certificates)
                .Where(application => application.Status == InstructorApplicationStatus.Approved
                    && application.User.IsActive
                    && !application.User.IsDeleted);

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(application => application.HourlyRate <= request.MaxPrice.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(application => application.HourlyRate >= request.MinPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Specialization))
            {
                var specialization = request.Specialization.Trim();
                query = query.Where(application => application.Specialization.Contains(specialization));
            }

            if (request.CountryId.HasValue)
            {
                query = query.Where(application => application.User.CountryId == request.CountryId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<InstructorCardDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<InstructorCardDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<InstructorCardDto>.MaxPageSize);

            var applications = await query
                .OrderBy(application => application.HourlyRate)
                .ThenByDescending(application => application.ReviewedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = applications
                .Select(MapToCard)
                .ToArray();

            var result = PagginatedResult<InstructorCardDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<InstructorCardDto>>.Success(
                result,
                _localizer[LocalizationKeys.Onboarding.InstructorsRetrieved].Value);
        }

        private static InstructorCardDto MapToCard(InstructorApplication application)
        {
            var certificates = application.Certificates
                .Where(certificate => certificate.Type != InstructorCertificateType.IntroVideo)
                .Select(certificate => certificate.FilePath)
                .ToArray();

            var introVideo = application.Certificates
                .Where(certificate => certificate.Type == InstructorCertificateType.IntroVideo)
                .OrderByDescending(certificate => certificate.CreatedAt)
                .Select(certificate => certificate.FilePath)
                .FirstOrDefault();

            return new InstructorCardDto(
                application.Id,
                application.UserId,
                application.User.FullName,
                application.Specialization,
                application.Bio,
                application.YearsOfExperience,
                application.Education,
                application.LinkedInUrl,
                application.HourlyRate,
                application.User.Country?.CountryEnName,
                introVideo,
                certificates,
                application.ReviewedAt ?? application.CreatedAt);
        }
    }
}
