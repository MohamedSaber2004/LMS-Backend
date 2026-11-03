using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.AddAvailabilitySlot
{
    public class AddAvailabilitySlotCommandHandler
        : IRequestHandler<AddAvailabilitySlotCommand, Result<AvailabilitySlotDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddAvailabilitySlotCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<AvailabilitySlotDto>> Handle(
            AddAvailabilitySlotCommand request,
            CancellationToken cancellationToken)
        {
            var slot = InstructorAvailabilitySlot.Create(
                request.InstructorId,
                request.Day,
                request.StartTime,
                request.EndTime);

            await _context.InstructorAvailabilitySlots.AddAsync(slot, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AvailabilitySlotDto>.Success(
                AvailabilityMapper.ToSlotDto(slot),
                _localizer[LocalizationKeys.Availability.SlotCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
