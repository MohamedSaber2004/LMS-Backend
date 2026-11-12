using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityWindow
{
    public class SetAvailabilityWindowCommandValidator : AbstractValidator<SetAvailabilityWindowCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetAvailabilityWindowCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Availability.SlotIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            SetAvailabilityWindowCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            if (instructorId is null)
            {
                context.AddFailure(
                    nameof(SetAvailabilityWindowCommand.Id),
                    _localizer[LocalizationKeys.ExceptionMessages.Forbidden].Value);
                return;
            }

            var instance = await _context.AvailabilityInstances
                .FirstOrDefaultAsync(
                    item => item.Id == command.Id && item.InstructorId == instructorId.Value,
                    cancellationToken);

            if (instance is null)
            {
                // Another instructor's row and a missing row are both reported as 404 by the handler.
                return;
            }

            var zone = await AvailabilityValidation.ResolveInstructorZoneAsync(
                _context, instructorId.Value, cancellationToken);

            if (zone is null)
            {
                context.AddFailure(
                    nameof(SetAvailabilityWindowCommand.Id),
                    _localizer[LocalizationKeys.Availability.TimezoneUnresolved].Value);
                return;
            }

            if (instance.Date < TimezoneHelper.LocalToday(zone))
            {
                context.AddFailure(
                    nameof(SetAvailabilityWindowCommand.Id),
                    _localizer[LocalizationKeys.Availability.DateInPast].Value);
            }
        }
    }
}
