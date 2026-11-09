using LMS.API.Services;
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
        private readonly ICurrentUserService _currentUserService;

        public GetInstructorsForLearnerQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PagginatedResult<InstructorCardDto>>> Handle(
            GetInstructorsForLearnerQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Instructors
                .AsNoTracking()
                .Include(instructor => instructor.Application)
                .Include(instructor => instructor.User)
                    .ThenInclude(user => user.Country)
                .Include(instructor => instructor.Certificates)
                .Where(instructor => instructor.Application != null
                    && instructor.Application.Status == InstructorApplicationStatus.Approved
                    && instructor.User.IsActive
                    && !instructor.User.IsDeleted);

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(instructor => instructor.HourlyRate <= request.MaxPrice.Value);
            }
            else if (_currentUserService.UserId != Guid.Empty)
            {
                var learner = await _context.Learners
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.UserId == _currentUserService.UserId, cancellationToken);

                if (learner?.HourlyRate.HasValue == true)
                {
                    query = query.Where(instructor => instructor.HourlyRate <= learner.HourlyRate.Value);
                }
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(instructor => instructor.HourlyRate >= request.MinPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Specialization))
            {
                var specialization = request.Specialization.Trim();
                query = query.Where(instructor => instructor.Specialization.Contains(specialization));
            }

            if (request.CountryId.HasValue)
            {
                query = query.Where(instructor => instructor.User.CountryId == request.CountryId.Value);
            }

            if (request.Day.HasValue)
            {
                var day = request.Day.Value;

                var after = request.After ?? new TimeSpan(23, 59, 59, 999);
                var before = request.Before ?? TimeSpan.Zero;

                query = query.Where(instructor => instructor.AvailabilitySlots.Any(slot =>
                    slot.Day == day
                    && slot.StartTime <= after
                    && slot.EndTime >= before));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<InstructorCardDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<InstructorCardDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<InstructorCardDto>.MaxPageSize);

            var instructors = await query
                .OrderBy(instructor => instructor.HourlyRate)
                .ThenByDescending(instructor => instructor.Application!.ReviewedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = instructors
                .Select(MapToCard)
                .ToArray();

            var result = PagginatedResult<InstructorCardDto>.Create(items, totalCount, pageNumber, pageSize);

            return Result<PagginatedResult<InstructorCardDto>>.Success(
                result,
                _localizer[LocalizationKeys.Onboarding.InstructorsRetrieved].Value);
        }

        private static InstructorCardDto MapToCard(Instructor instructor)
        {
            var certificates = instructor.Certificates
                .Where(certificate => certificate.Type != InstructorCertificateType.IntroVideo)
                .Select(certificate => certificate.FilePath)
                .ToArray();

            var introVideo = instructor.Certificates
                .Where(certificate => certificate.Type == InstructorCertificateType.IntroVideo)
                .OrderByDescending(certificate => certificate.CreatedAt)
                .Select(certificate => certificate.FilePath)
                .FirstOrDefault();

            return new InstructorCardDto(
                instructor.Id,
                instructor.UserId,
                instructor.User.FullName,
                instructor.Specialization,
                instructor.Bio,
                instructor.YearsOfExperience,
                instructor.Education,
                instructor.LinkedInUrl,
                instructor.HourlyRate,
                instructor.User.Country?.CountryEnName,
                introVideo,
                certificates,
                instructor.Application?.ReviewedAt ?? instructor.CreatedAt);
        }
    }
}
