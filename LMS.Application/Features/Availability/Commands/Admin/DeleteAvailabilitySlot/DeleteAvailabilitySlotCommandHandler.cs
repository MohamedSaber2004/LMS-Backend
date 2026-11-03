using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandHandler
        : IRequestHandler<DeleteAvailabilitySlotCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeleteAvailabilitySlotCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            DeleteAvailabilitySlotCommand request,
            CancellationToken cancellationToken)
        {
            var slot = await _context.InstructorAvailabilitySlots
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.SlotNotFound);

            slot.MarkAsDeleted(_currentUserService.UserId.ToString());
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Availability.SlotDeletedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
