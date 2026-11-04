using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.SetVacationMode
{
    public class SetVacationModeCommandHandler : IRequestHandler<SetVacationModeCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetVacationModeCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            SetVacationModeCommand request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instructor = await _context.Instructors
                .FirstAsync(item => item.Id == instructorId, cancellationToken);

            // Only new bookings are shielded. Confirmed sessions keep their slots and their 24 h rule.
            instructor.SetVacationMode(request.Enabled);
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Availability.VacationModeUpdatedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
