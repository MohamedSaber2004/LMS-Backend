using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Commands.SetInstructorPricingAdmin
{
    public class SetInstructorPricingAdminCommand : IRequest<Result<string>>
    {
        public Guid InstructorId { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal CommissionRate { get; set; } // CCR
        public bool? IsNativeSpeaker { get; set; }
        public List<string>? Languages { get; set; }
    }

    public class SetInstructorPricingAdminCommandValidator : AbstractValidator<SetInstructorPricingAdminCommand>
    {
        public SetInstructorPricingAdminCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Pricing.InstructorIdRequired].Value);

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Pricing.HourlyRateMustBePositive].Value);

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 100)
                .WithMessage(localizer[LocalizationKeys.Pricing.CommissionRateRange].Value);
        }
    }

    public class SetInstructorPricingAdminCommandHandler : IRequestHandler<SetInstructorPricingAdminCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetInstructorPricingAdminCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            SetInstructorPricingAdminCommand request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            instructor.SetAdminPricing(
                request.HourlyRate,
                request.CommissionRate,
                request.IsNativeSpeaker,
                request.Languages);

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Pricing.PricingUpdatedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}

