using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.Admin.AddAvailabilitySlot
{
    public class AddAvailabilitySlotCommandValidator : AbstractValidator<AddAvailabilitySlotCommand>
    {
        private readonly ILmsDbContext _context;

        public AddAvailabilitySlotCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            RuleFor(command => command.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorIdRequired].Value)
                .MustAsync((instructorId, cancellationToken) =>
                    AvailabilityValidation.InstructorExistsAsync(_context, instructorId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorNotFound].Value)
                .MustAsync((instructorId, cancellationToken) =>
                    AvailabilityValidation.InstructorIsApprovedAsync(_context, instructorId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorNotApproved].Value);

            RuleFor(command => command.Day)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Availability.DayInvalid].Value);

            RuleFor(command => command.EndTime)
                .GreaterThan(command => command.StartTime)
                .WithMessage(localizer[LocalizationKeys.Availability.EndTimeAfterStartTime].Value);

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) =>
                    AvailabilityValidation.HasNoOverlapAsync(
                        _context,
                        command.InstructorId,
                        command.Day,
                        command.StartTime,
                        command.EndTime,
                        excludedSlotId: null,
                        cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.OverlappingSlot].Value)
                .When(command => command.EndTime > command.StartTime);
        }
    }
}
