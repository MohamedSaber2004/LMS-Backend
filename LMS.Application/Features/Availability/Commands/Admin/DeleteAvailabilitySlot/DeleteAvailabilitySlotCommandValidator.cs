using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandValidator : AbstractValidator<DeleteAvailabilitySlotCommand>
    {
        private readonly ILmsDbContext _context;

        public DeleteAvailabilitySlotCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.SlotIdRequired].Value);

            RuleFor(command => command.Id)
                .MustAsync((slotId, cancellationToken) =>
                    AvailabilityValidation.HasNoLiveSessionsAsync(_context, slotId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.SlotHasSessions].Value);
        }
    }
}
