using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityWindow
{
    public class SetAvailabilityWindowCommandHandler
        : IRequestHandler<SetAvailabilityWindowCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetAvailabilityWindowCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            SetAvailabilityWindowCommand request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instance = await _context.AvailabilityInstances
                .FirstOrDefaultAsync(
                    item => item.Id == request.Id && item.InstructorId == instructorId,
                    cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.SlotNotFound);

            if (request.Open)
            {
                instance.Open();
            }
            else
            {
                instance.Block();
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Availability.WindowUpdatedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
