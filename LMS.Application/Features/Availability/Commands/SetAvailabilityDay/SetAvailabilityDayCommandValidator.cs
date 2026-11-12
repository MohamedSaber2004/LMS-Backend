using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityDay
{
    public class SetAvailabilityDayCommandValidator : AbstractValidator<SetAvailabilityDayCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetAvailabilityDayCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            SetAvailabilityDayCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken);

            if (instructorId is null)
            {
                context.AddFailure(
                    nameof(SetAvailabilityDayCommand.Date),
                    _localizer[LocalizationKeys.ExceptionMessages.Forbidden].Value);
                return;
            }

            var zone = await AvailabilityValidation.ResolveInstructorZoneAsync(
                _context, instructorId.Value, cancellationToken);

            if (zone is null)
            {
                context.AddFailure(
                    nameof(SetAvailabilityDayCommand.Date),
                    _localizer[LocalizationKeys.Availability.TimezoneUnresolved].Value);
                return;
            }

            if (command.Date < TimezoneHelper.LocalToday(zone))
            {
                context.AddFailure(
                    nameof(SetAvailabilityDayCommand.Date),
                    _localizer[LocalizationKeys.Availability.DateInPast].Value);
                return;
            }

            var hasWindows = await _context.AvailabilityInstances.AnyAsync(
                instance => instance.InstructorId == instructorId.Value
                    && instance.Date == command.Date,
                cancellationToken);

            if (!hasWindows)
            {
                context.AddFailure(
                    nameof(SetAvailabilityDayCommand.Date),
                    _localizer[LocalizationKeys.Availability.NoWindowsOnDay].Value);
            }
        }
    }
}
