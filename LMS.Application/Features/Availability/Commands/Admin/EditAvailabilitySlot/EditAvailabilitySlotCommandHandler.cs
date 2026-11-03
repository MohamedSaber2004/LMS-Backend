using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.EditAvailabilitySlot
{
    public class EditAvailabilitySlotCommandHandler
        : IRequestHandler<EditAvailabilitySlotCommand, Result<AvailabilitySlotDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public EditAvailabilitySlotCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<AvailabilitySlotDto>> Handle(
            EditAvailabilitySlotCommand request,
            CancellationToken cancellationToken)
        {
            var slot = await _context.InstructorAvailabilitySlots
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.SlotNotFound);

            slot.Update(request.Day, request.StartTime, request.EndTime);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AvailabilitySlotDto>.Success(
                AvailabilityMapper.ToSlotDto(slot),
                _localizer[LocalizationKeys.Availability.SlotUpdatedSuccess].Value);
        }
    }
}
