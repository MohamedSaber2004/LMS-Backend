using LMS.API.Services;
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

namespace LMS.Application.Features.Availability.Commands.SetTeacherVacationMode
{
    public class SetTeacherVacationModeCommandHandler
        : IRequestHandler<SetTeacherVacationModeCommand, Result<TeacherVacationModeResultDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVacationJobScheduler _vacationJobScheduler;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetTeacherVacationModeCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IVacationJobScheduler vacationJobScheduler,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _vacationJobScheduler = vacationJobScheduler;
            _localizer = localizer;
        }

        public async Task<Result<TeacherVacationModeResultDto>> Handle(
            SetTeacherVacationModeCommand request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instructor = await _context.Instructors
                .FirstAsync(item => item.Id == instructorId, cancellationToken);

            instructor.SetVacationMode(request.Enabled);

            var hasConflicts = false;
            var conflictCount = 0;

            if (request.Enabled)
            {
                var now = DateTime.UtcNow;
                conflictCount = await _context.Sessions
                    .CountAsync(s => s.InstructorId == instructorId
                        && (s.Status == SessionStatus.Confirmed || s.Status == SessionStatus.Pending)
                        && s.EndsAtUtc > now,
                        cancellationToken);

                hasConflicts = conflictCount > 0;

                if (hasConflicts)
                {
                    _vacationJobScheduler.EnqueueVacationConflictProcessing(instructorId);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Availability.VacationModeUpdatedSuccess].Value;

            var result = new TeacherVacationModeResultDto
            {
                VacationModeEnabled = request.Enabled,
                HasConflicts = hasConflicts,
                ConflictCount = conflictCount,
                Message = message
            };

            return Result<TeacherVacationModeResultDto>.Success(result, message);
        }
    }
}

