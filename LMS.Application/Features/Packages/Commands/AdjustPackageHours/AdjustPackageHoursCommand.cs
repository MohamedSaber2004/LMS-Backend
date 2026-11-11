using System.Text.Json.Serialization;
using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.AdjustPackageHours
{
    public class AdjustPackageHoursCommand : IRequest<Result<LearnerPackageDto>>
    {
        [JsonIgnore]
        public Guid PackageId { get; set; }
        public decimal NewRemainingHours { get; set; }
        public string? Reason { get; set; }
    }

    public class AdjustPackageHoursCommandValidator : AbstractValidator<AdjustPackageHoursCommand>
    {
        private const int ReasonMaxLength = 500;

        public AdjustPackageHoursCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.NewRemainingHours)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Packages.PackageAdjustmentHoursNegative].Value);

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.PackageAdjustmentReasonRequired].Value)
                .MaximumLength(ReasonMaxLength)
                .WithMessage(localizer[LocalizationKeys.Packages.PackageAdjustmentReasonTooLong, ReasonMaxLength].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.PackageId == Guid.Empty)
                    {
                        return;
                    }

                    var totalHours = await context.LearnerPackages
                        .AsNoTracking()
                        .Where(package => package.Id == command.PackageId)
                        .Select(package => (decimal?)package.TotalHours)
                        .FirstOrDefaultAsync(cancellationToken);

                    // A missing package is reported by the handler so the response stays a 404 with the id.
                    if (totalHours is null || command.NewRemainingHours <= totalHours.Value)
                    {
                        return;
                    }

                    validationContext.AddFailure(
                        nameof(command.NewRemainingHours),
                        localizer[
                            LocalizationKeys.Packages.PackageHoursExceedTotal,
                            command.NewRemainingHours,
                            totalHours.Value].Value);
                });
        }
    }

    public class AdjustPackageHoursCommandHandler : IRequestHandler<AdjustPackageHoursCommand, Result<LearnerPackageDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AdjustPackageHoursCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<LearnerPackageDto>> Handle(
            AdjustPackageHoursCommand request,
            CancellationToken cancellationToken)
        {
            var package = await _context.LearnerPackages
                .Include(lp => lp.Instructor!)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(lp => lp.Id == request.PackageId, cancellationToken)
                ?? throw new NotFoundException(nameof(LearnerPackage), request.PackageId);

            var oldRemainingHours = package.RemainingHours;

            package.AdjustHours(request.NewRemainingHours);

            _context.PackageHourAdjustments.Add(PackageHourAdjustment.Create(
                package.Id,
                oldRemainingHours,
                package.RemainingHours,
                request.Reason!));

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new LearnerPackageDto
            {
                Id = package.Id,
                InstructorId = package.InstructorId,
                InstructorFullName = package.Instructor != null && package.Instructor.User != null ? package.Instructor.User.FullName : "All Instructors",
                PackageName = package.PackageName,
                TotalHours = package.TotalHours,
                RemainingHours = package.RemainingHours,
                HourlyRateSnapshot = package.HourlyRateSnapshot,
                TotalPrice = package.TotalPrice,
                IsActive = package.IsActive,
                CreatedAt = package.CreatedAt
            };

            return Result<LearnerPackageDto>.Success(
                dto,
                _localizer[LocalizationKeys.AdminManagement.PackageAdjustedSuccess].Value);
        }
    }
}
