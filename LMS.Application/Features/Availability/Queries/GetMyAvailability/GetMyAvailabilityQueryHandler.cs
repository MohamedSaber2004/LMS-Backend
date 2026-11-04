using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.GetMyAvailability
{
    public class GetMyAvailabilityQueryHandler
        : IRequestHandler<GetMyAvailabilityQuery, Result<MyAvailabilityDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyAvailabilityQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<MyAvailabilityDto>> Handle(
            GetMyAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.AvailabilitySlots)
                .FirstAsync(item => item.Id == instructorId, cancellationToken);

            var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                _context, instructor.Id, cancellationToken);

            var weeklySlots = instructor.AvailabilitySlots
                .OrderBy(slot => slot.Day)
                .ThenBy(slot => slot.StartTime)
                .Select(AvailabilityMapper.ToSlotDto)
                .ToArray();

            var response = new MyAvailabilityDto
            {
                InstructorId = instructor.Id,
                TimezoneId = timezoneId,
                VacationMode = instructor.VacationMode,
                WeeklySlots = weeklySlots
            };

            return Result<MyAvailabilityDto>.Success(
                response,
                _localizer[LocalizationKeys.Availability.AvailabilityRetrieved].Value);
        }
    }
}
