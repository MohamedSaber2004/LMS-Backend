using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Queries.GetInstructorDetailsForLearner
{
    public class GetInstructorDetailsForLearnerQuery : IRequest<Result<InstructorCardDto>>
    {
        public Guid InstructorId { get; set; }
    }

    public class GetInstructorDetailsForLearnerQueryHandler : IRequestHandler<GetInstructorDetailsForLearnerQuery, Result<InstructorCardDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorDetailsForLearnerQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<InstructorCardDto>> Handle(
            GetInstructorDetailsForLearnerQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(i => i.Application)
                .Include(i => i.User)
                    .ThenInclude(u => u.Country)
                .Include(i => i.Certificates)
                .FirstOrDefaultAsync(i => i.Id == request.InstructorId
                    && i.Application != null
                    && i.Application.Status == InstructorApplicationStatus.Approved
                    && i.User.IsActive
                    && !i.User.IsDeleted,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var certificates = instructor.Certificates
                .Where(c => c.Type != InstructorCertificateType.IntroVideo)
                .Select(c => c.FilePath)
                .ToArray();

            var introVideo = instructor.Certificates
                .Where(c => c.Type == InstructorCertificateType.IntroVideo)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => c.FilePath)
                .FirstOrDefault();

            var dto = new InstructorCardDto(
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
                instructor.Application?.ReviewedAt ?? instructor.CreatedAt,
                instructor.IsNativeSpeaker,
                instructor.Rank,
                instructor.GetLanguagesList(),
                instructor.RatingAverage,
                instructor.TotalReviews,
                instructor.CompletedSessionsCount);

            return Result<InstructorCardDto>.Success(
                dto,
                _localizer[LocalizationKeys.Onboarding.InstructorDetailsRetrieved].Value);
        }
    }
}

