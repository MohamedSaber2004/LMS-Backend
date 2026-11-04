using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityDay
{
    public class SetAvailabilityDayCommandHandler : IRequestHandler<SetAvailabilityDayCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetAvailabilityDayCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            SetAvailabilityDayCommand request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instances = await _context.AvailabilityInstances
                .Where(instance => instance.InstructorId == instructorId && instance.Date == request.Date)
                .ToListAsync(cancellationToken);

            foreach (var instance in instances)
            {
                if (request.Open)
                {
                    instance.Open();
                }
                else
                {
                    instance.Block();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Availability.DayUpdatedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
