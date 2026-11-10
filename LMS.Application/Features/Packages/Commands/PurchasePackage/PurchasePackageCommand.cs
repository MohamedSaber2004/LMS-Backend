using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.PurchasePackage
{
    public class PurchasePackageCommand : IRequest<Result<LearnerPackageDto>>
    {
        public Guid InstructorId { get; set; }
        public decimal TotalHours { get; set; }
    }

    public class PurchasePackageCommandValidator : AbstractValidator<PurchasePackageCommand>
    {
        public PurchasePackageCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.InstructorId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.InstructorIdRequired].Value);

            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Packages.TierHoursMustBePositive].Value)
                .CustomAsync(async (hours, validationContext, cancellationToken) =>
                {
                    if (hours <= 0)
                    {
                        return;
                    }

                    var availableHours = await context.PackageTiers
                        .AsNoTracking()
                        .OrderBy(tier => tier.DisplayOrder)
                        .Select(tier => tier.TotalHours)
                        .ToListAsync(cancellationToken);

                    if (availableHours.Contains(hours))
                    {
                        return;
                    }

                    validationContext.AddFailure(
                        localizer[
                            LocalizationKeys.Packages.TierNotFound,
                            hours.ToString("0.##"),
                            string.Join(", ", availableHours.Select(hour => hour.ToString("0.##")))].Value);
                });
        }
    }

    public class PurchasePackageCommandHandler : IRequestHandler<PurchasePackageCommand, Result<LearnerPackageDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public PurchasePackageCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<LearnerPackageDto>> Handle(
            PurchasePackageCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var tiers = await _context.PackageTiers
                .AsNoTracking()
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync(cancellationToken);

            var tier = tiers.FirstOrDefault(t => t.TotalHours == request.TotalHours)
                ?? throw new NotFoundException(
                    LocalizationKeys.Packages.TierNotFound,
                    request.TotalHours.ToString("0.##"),
                    string.Join(", ", tiers.Select(t => t.TotalHours.ToString("0.##"))));

            var package = LearnerPackage.Create(
                learnerId,
                instructor.Id,
                tier.Name,
                tier.TotalHours,
                instructor.HourlyRate);

            await _context.LearnerPackages.AddAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new LearnerPackageDto
            {
                Id = package.Id,
                InstructorId = instructor.Id,
                InstructorFullName = instructor.User?.FullName ?? string.Empty,
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
                _localizer[LocalizationKeys.Packages.PurchaseSuccess].Value);
        }
    }
}

