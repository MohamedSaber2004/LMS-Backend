using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.EditAvailabilitySlot
{
    public class EditAvailabilitySlotCommandValidator : AbstractValidator<EditAvailabilitySlotCommand>
    {
        private readonly ILmsDbContext _context;

        public EditAvailabilitySlotCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.SlotIdRequired].Value);

            RuleFor(command => command.Day)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Availability.DayInvalid].Value);

            RuleFor(command => command.EndTime)
                .GreaterThan(command => command.StartTime)
                .WithMessage(localizer[LocalizationKeys.Availability.EndTimeAfterStartTime].Value);

            RuleFor(command => command.Id)
                .MustAsync((slotId, cancellationToken) =>
                    AvailabilityValidation.HasNoLiveSessionsAsync(_context, slotId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.SlotHasSessions].Value);

            // The slot keeps its instructor, so the overlap check reads the owner from the stored row.
            // A missing row is reported as 404 by the handler.
            RuleFor(command => command)
                .MustAsync(async (command, cancellationToken) =>
                {
                    var instructorId = await _context.InstructorAvailabilitySlots
                        .Where(slot => slot.Id == command.Id)
                        .Select(slot => slot.InstructorId)
                        .FirstOrDefaultAsync(cancellationToken);

                    return await AvailabilityValidation.HasNoOverlapAsync(
                        _context,
                        instructorId,
                        command.Day,
                        command.StartTime,
                        command.EndTime,
                        excludedSlotId: command.Id,
                        cancellationToken);
                })
                .WithMessage(localizer[LocalizationKeys.Availability.OverlappingSlot].Value)
                .When(command => command.EndTime > command.StartTime);
        }
    }
}
