using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Helpers;
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
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;

            // Only an admin names the instructor whose slot this is. Any other caller is pinned to
            // their own instructor row by the handler, so a supplied id is not read from them.
            RuleFor(command => command.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorIdRequired].Value)
                .MustAsync((instructorId, cancellationToken) =>
                    AvailabilityValidation.InstructorExistsAsync(_context, instructorId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorNotFound].Value)
                .MustAsync((instructorId, cancellationToken) =>
                    AvailabilityValidation.InstructorIsApprovedAsync(_context, instructorId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.Availability.InstructorNotApproved].Value)
                .When(command => ActorOwnership.IsAdmin(currentUser));

            RuleFor(command => command.Day)
                .IsInEnum()
                .WithMessage(localizer[LocalizationKeys.Availability.DayInvalid].Value);

            RuleFor(command => command.EndTime)
                .GreaterThan(command => command.StartTime)
                .WithMessage(localizer[LocalizationKeys.Availability.EndTimeAfterStartTime].Value);

            RuleFor(command => command)
                .MustAsync(async (command, cancellationToken) =>
                {
                    var instructorId = await ActorOwnership.ResolveWriteTargetAsync(
                        _context, currentUser, command.InstructorId, cancellationToken);

                    return await AvailabilityValidation.HasNoOverlapAsync(
                        _context,
                        instructorId,
                        command.Day,
                        command.StartTime,
                        command.EndTime,
                        excludedSlotId: null,
                        cancellationToken);
                })
                .WithMessage(localizer[LocalizationKeys.Availability.OverlappingSlot].Value)
                .When(command => command.EndTime > command.StartTime);
        }
    }
}
