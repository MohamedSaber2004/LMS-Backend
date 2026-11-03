using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandValidator : AbstractValidator<DeleteAvailabilitySlotCommand>
    {
        public DeleteAvailabilitySlotCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.SlotIdRequired].Value);
        }
    }
}
