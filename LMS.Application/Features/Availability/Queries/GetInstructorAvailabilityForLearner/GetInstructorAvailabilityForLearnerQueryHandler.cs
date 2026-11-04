using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityForLearner
{
    public class GetInstructorAvailabilityForLearnerQueryHandler
        : IRequestHandler<GetInstructorAvailabilityForLearnerQuery, Result<InstructorAvailabilityDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorAvailabilityForLearnerQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<InstructorAvailabilityDto>> Handle(
            GetInstructorAvailabilityForLearnerQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.User)
                    .ThenInclude(user => user.Country)
                .Include(item => item.Application)
                .Include(item => item.AvailabilitySlots)
                .FirstOrDefaultAsync(
                    item => item.Id == request.InstructorId
                        && item.Application != null
                        && item.Application.Status == InstructorApplicationStatus.Approved
                        && item.User.IsActive
                        && !item.User.IsDeleted,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var weeklySlots = instructor.AvailabilitySlots
                .OrderBy(slot => slot.Day)
                .ThenBy(slot => slot.StartTime)
                .Select(AvailabilityMapper.ToSlotDto)
                .ToArray();

            var response = new InstructorAvailabilityDto
            {
                InstructorId = instructor.Id,
                UserId = instructor.UserId,
                FullName = instructor.User.FullName,
                CountryId = instructor.User.CountryId,
                CountryCode = instructor.User.Country?.Code,
                CountryName = instructor.User.Country?.CountryEnName,
                TimezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                    _context, instructor.Id, cancellationToken),
                HourlyRate = instructor.HourlyRate,
                WeeklySlots = weeklySlots
            };

            return Result<InstructorAvailabilityDto>.Success(
                response,
                _localizer[LocalizationKeys.Availability.AvailabilityRetrieved].Value);
        }
    }
}
